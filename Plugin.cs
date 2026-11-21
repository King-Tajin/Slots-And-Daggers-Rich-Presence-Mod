using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using BepInEx;
using BepInEx.Configuration;
using DiscordRPC;
using UnityEngine;

namespace DiscordPresence
{
    [BepInPlugin("com.king_tajin.discordpresence", "Discord Presence", "0.6.0")]
    public class Plugin : BaseUnityPlugin
    {
        private ConfigEntry<string> applicationId;
        private ConfigEntry<string> largeImageKey;
        private ConfigEntry<bool> showEnemyImage;
        private ConfigEntry<bool> showHealth;
        private ConfigEntry<bool> showScore;
        private ConfigEntry<float> pollInterval;
        private ConfigEntry<float> minUpdateInterval;
        private ConfigEntry<float> enemyLingerSeconds;

        private readonly HashSet<string> warnedKeys = new HashSet<string>();
        private DiscordRpcClient client;
        private Timestamps startTime;
        private float nextPoll;
        private float nextSend;
        private string lingerEnemy;
        private float lingerUntil;
        private string lastDetails;
        private string lastState;
        private string lastEnemy;
        private string lastLoggedNameIssue;
        private bool runActive;
        private bool errorLogged;

        private void Awake()
        {
            applicationId = Config.Bind("General", "ApplicationId", "1573513660431859874", "Discord application ID from the Developer Portal");
            largeImageKey = Config.Bind("General", "LargeImageKey", "logo", "Art asset key used for the logo, shown when no enemy image applies");
            showEnemyImage = Config.Bind("Display", "ShowEnemyImage", true, "Show the art of the enemy you are fighting");
            showHealth = Config.Bind("Display", "ShowHealth", true, "Show player health as a percentage in the presence");
            showScore = Config.Bind("Display", "ShowScore", true, "Show the current score in the presence");
            pollInterval = Config.Bind("Display", "PollIntervalSeconds", 2f, "How often the game state is checked");
            enemyLingerSeconds = Config.Bind("Display", "EnemyLingerSeconds", 8f, "How long the last enemy stays shown after a fight ends, before switching to Between fights");
            minUpdateInterval = Config.Bind("Display", "MinUpdateIntervalSeconds", 5f, "Minimum time between presence updates, to stay under Discord rate limits");

            if (string.IsNullOrWhiteSpace(applicationId.Value) || applicationId.Value == "0")
            {
                Logger.LogWarning("Set ApplicationId in the config file, then restart the game.");
                enabled = false;
                return;
            }

            client = new DiscordRpcClient(applicationId.Value);
            client.Initialize();
            startTime = Timestamps.Now;
            Apply("Starting up", null, null);
            Logger.LogInfo("Discord Presence loaded");
        }

        private void Update()
        {
            if (client == null || Time.unscaledTime < nextPoll)
            {
                return;
            }

            nextPoll = Time.unscaledTime + Mathf.Max(0.5f, pollInterval.Value);

            try
            {
                Refresh();
            }
            catch (Exception e)
            {
                if (!errorLogged)
                {
                    errorLogged = true;
                    Logger.LogError(e);
                }
            }
        }

        private void Refresh()
        {
            GameManager gm = Main.GameManager;
            if (gm == null)
            {
                Apply("Starting up", null, null);
                return;
            }

            GameState state = gm.CurrentGameState;

            switch (state)
            {
                case GameState.SplashScreen:
                case GameState.LevelSelect:
                case GameState.EquipmentSelect:
                case GameState.NoGameSession:
                case GameState.NoState:
                case GameState.StartNewGame:
                case GameState.ResetGame:
                case GameState.GameOver:
                case GameState.GameWon:
                case GameState.DemoWon:
                    runActive = false;
                    lingerEnemy = null;
                    Apply("In the menus", null, null);
                    return;
            }

            if (!gm.GamePaused)
            {
                runActive = true;
            }

            if (!runActive)
            {
                lingerEnemy = null;
                Apply("In the menus", null, null);
                return;
            }

            bool endless = gm.CurrentGameMode == GameMode.Endless;
            string details = endless ? "Arena" : LevelName();

            string status;
            string enemy = null;
            if (state == GameState.UpgradeShop)
            {
                lingerEnemy = null;
                status = "Visiting the shop";
            }
            else
            {
                Encounter encounter = CurrentEncounter();
                if (encounter != null)
                {
                    enemy = ResolveEnemyName(encounter);
                    if (string.IsNullOrEmpty(enemy))
                    {
                        lingerEnemy = null;
                        status = "In a fight";
                    }
                    else
                    {
                        lingerEnemy = enemy;
                        lingerUntil = Time.unscaledTime + Mathf.Max(0f, enemyLingerSeconds.Value);
                        status = "Fighting " + enemy;
                    }
                }
                else if (lingerEnemy != null && Time.unscaledTime < lingerUntil)
                {
                    enemy = lingerEnemy;
                    status = "Fighting " + enemy;
                }
                else
                {
                    lingerEnemy = null;
                    status = "Between fights";
                }
            }

            string stats = BuildStats();
            if (!string.IsNullOrEmpty(stats))
            {
                status += " | " + stats;
            }

            Apply(details, status, enemy);
        }

        private string LevelName()
        {
            LevelManager lm = Main.LevelManager;
            if (lm == null || lm.currentLevel == null || string.IsNullOrEmpty(lm.currentLevel.displayName))
            {
                return "Slots & Daggers";
            }

            return lm.currentLevel.displayName;
        }

        private Encounter CurrentEncounter()
        {
            EncounterManager em = Main.EncounterManager;
            return em == null ? null : em.CurrentEncounter;
        }

        private string ResolveEnemyName(Encounter encounter)
        {
            if (!string.IsNullOrWhiteSpace(encounter.displayName))
            {
                return encounter.displayName.Trim();
            }

            string localized = LocalizedName(encounter);
            if (!string.IsNullOrWhiteSpace(localized))
            {
                LogNameIssue(encounter, "displayName is empty, using localized name \"" + localized + "\"");
                return localized.Trim();
            }

            string objectName = encounter.name == null ? null : encounter.name.Replace("(Clone)", "").Trim();
            LogNameIssue(encounter, "displayName and localized name are empty, object name is \"" + objectName + "\"");
            return string.IsNullOrWhiteSpace(objectName) ? null : objectName;
        }

        private static string LocalizedName(Encounter encounter)
        {
            try
            {
                FieldInfo field = typeof(Encounter).GetField("displayNameLocalized", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                object localized = field == null ? null : field.GetValue(encounter);
                if (localized == null)
                {
                    return null;
                }

                MethodInfo method = localized.GetType().GetMethod("GetLocalizedString", Type.EmptyTypes);
                return method == null ? null : method.Invoke(localized, null) as string;
            }
            catch
            {
                return null;
            }
        }

        private void LogNameIssue(Encounter encounter, string message)
        {
            string id = encounter.GetInstanceID() + message;
            if (id == lastLoggedNameIssue)
            {
                return;
            }

            lastLoggedNameIssue = id;
            Logger.LogWarning("Enemy name fallback: " + message);
        }

        private static string ToAssetKey(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            StringBuilder sb = new StringBuilder();
            bool pendingDash = false;

            foreach (char c in name.Trim().ToLowerInvariant())
            {
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
                {
                    if (pendingDash && sb.Length > 0)
                    {
                        sb.Append('-');
                    }

                    pendingDash = false;
                    sb.Append(c);
                }
                else if (c == '\'' || c == '\u2019')
                {
                    continue;
                }
                else
                {
                    pendingDash = true;
                }
            }

            return sb.Length == 0 ? null : sb.ToString();
        }

        private string BuildStats()
        {
            string result = null;

            if (showHealth.Value && Main.HealthManager != null)
            {
                float max = Main.HealthManager.MaxHealth;
                int percent = max > 0f ? Mathf.Clamp(Mathf.RoundToInt(Main.HealthManager.CurrentHealth / max * 100f), 0, 100) : 0;
                result = "HP " + percent + "%";
            }

            if (showScore.Value && Main.ScoreManager != null)
            {
                string score = "Score " + Main.ScoreManager.CurrentScore.ToString("N0");
                result = result == null ? score : result + ", " + score;
            }

            return result;
        }

        private void Apply(string details, string state, string enemy)
        {
            if (client == null || (details == lastDetails && state == lastState && enemy == lastEnemy))
            {
                return;
            }

            if (Time.unscaledTime < nextSend)
            {
                return;
            }

            nextSend = Time.unscaledTime + Mathf.Max(0f, minUpdateInterval.Value);
            lastDetails = details;
            lastState = state;
            lastEnemy = enemy;

            string enemyKey = showEnemyImage.Value ? ToAssetKey(enemy) : null;
            if (enemyKey != null && EnemyAssets.Aliases.TryGetValue(enemyKey, out string alias))
            {
                enemyKey = alias;
            }

            bool hasEnemyArt = enemyKey != null && EnemyAssets.Known.Contains(enemyKey);

            if (enemyKey != null && !hasEnemyArt && warnedKeys.Add(enemyKey))
            {
                Logger.LogWarning("No art asset found for enemy \"" + enemy + "\" (key \"" + enemyKey + "\"), using the logo.");
            }

            Assets assets = new Assets();

            if (hasEnemyArt)
            {
                assets.LargeImageKey = enemyKey;
                assets.LargeImageText = enemy;
                assets.SmallImageKey = largeImageKey.Value;
                assets.SmallImageText = "Slots & Daggers";
            }
            else
            {
                assets.LargeImageKey = largeImageKey.Value;
                assets.LargeImageText = "Slots & Daggers";
            }

            client.SetPresence(new RichPresence
            {
                Details = details,
                State = state,
                Timestamps = startTime,
                Assets = assets
            });
        }

        private void OnDestroy()
        {
            Shutdown();
        }

        private void OnApplicationQuit()
        {
            Shutdown();
        }

        private void Shutdown()
        {
            if (client == null)
            {
                return;
            }

            client.ClearPresence();
            client.Dispose();
            client = null;
        }
    }
}
