using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;

namespace SentoLauncher.Models
{
    public class LauncherConfig
    {
        [JsonProperty("version")]
        public int Version { get; set; } = 1;

        [JsonProperty("lastUpdated")]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        [JsonProperty("accounts")]
        public List<AccountInfo> Accounts { get; set; } = new();

        [JsonProperty("selectedAccountId")]
        public string? SelectedAccountId { get; set; }

        [JsonProperty("settings")]
        public LauncherSettings Settings { get; set; } = new();

        [JsonProperty("gameVersions")]
        public List<GameVersionInfo> GameVersions { get; set; } = new();

        [JsonProperty("selectedVersionId")]
        public string? SelectedVersionId { get; set; }

        [JsonProperty("profiles")]
        public List<ProfileInfo> Profiles { get; set; } = new();

        [JsonProperty("selectedProfileId")]
        public string? SelectedProfileId { get; set; }
    }

    /// <summary>
    /// A user-created game profile (like an "instance").
    /// A profile pins a Minecraft version plus an optional mod loader
    /// (Vanilla, Fabric, Quilt, Legacy Fabric, Forge, NeoForge) and
    /// uses the shared game directory.
    /// </summary>
    public class ProfileInfo
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("name")]
        public string Name { get; set; } = "Yeni Profil";

        [JsonProperty("minecraftVersion")]
        public string MinecraftVersion { get; set; } = string.Empty;

        [JsonProperty("loader")]
        public string Loader { get; set; } = "vanilla"; // vanilla | fabric | quilt | legacyfabric | forge | neoforge

        [JsonProperty("loaderVersion")]
        public string? LoaderVersion { get; set; }

        [JsonProperty("javaPath")]
        public string? JavaPath { get; set; }

        /// <summary>Preferred Java major version: "auto" (from the game version) or a specific major.</summary>
        [JsonProperty("javaVersion")]
        public string JavaVersion { get; set; } = "auto"; // auto | 25 | 21 | 17 | 8

        [JsonProperty("jvmArguments")]
        public string? JvmArguments { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonProperty("lastPlayed")]
        public DateTime? LastPlayed { get; set; }

        /// <summary>Runtime-composed version id: e.g. "1.21.1", "1.21.1-fabric-0.16.5",
        /// "1.20.1-forge-47.2.0".</summary>
        [JsonIgnore]
        public string VersionId => Loader == "vanilla" || string.IsNullOrEmpty(Loader)
            ? MinecraftVersion
            : $"{MinecraftVersion}-{Loader}-{LoaderVersion}";

        [JsonIgnore]
        public bool IsFabric => Loader == "fabric";

        /// <summary>Loaders that install via inheritsFrom profile json (Fabric-style).</summary>
        [JsonIgnore]
        public bool IsProfileStyleLoader => Loader is "fabric" or "quilt" or "legacyfabric";

        /// <summary>Loaders that install via installer.jar (Forge-style).</summary>
        [JsonIgnore]
        public bool IsInstallerStyleLoader => Loader is "forge" or "neoforge";

        [JsonIgnore]
        public string LoaderDisplayName => Loader switch
        {
            "fabric" => "Fabric",
            "quilt" => "Quilt",
            "legacyfabric" => "Legacy Fabric",
            "forge" => "Forge",
            "neoforge" => "NeoForge",
            _ => "Vanilla"
        };

        [JsonIgnore]
        public string Initial => string.IsNullOrWhiteSpace(Name) ? "P" : Name.Trim().Substring(0, 1).ToUpperInvariant();
    }

    public class AccountInfo
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("type")]
        public AuthType Type { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; } = string.Empty;

        [JsonProperty("displayName")]
        public string? DisplayName { get; set; }

        [JsonProperty("uuid")]
        public string? Uuid { get; set; }

        [JsonProperty("accessToken")]
        public string? AccessToken { get; set; }

        [JsonProperty("refreshToken")]
        public string? RefreshToken { get; set; }

        [JsonProperty("tokenExpiry")]
        public DateTime? TokenExpiry { get; set; }

        [JsonProperty("microsoftId")]
        public string? MicrosoftId { get; set; }

        [JsonProperty("avatarUrl")]
        public string? AvatarUrl { get; set; }

        [JsonProperty("lastLogin")]
        public DateTime LastLogin { get; set; } = DateTime.UtcNow;

        [JsonProperty("isSelected")]
        public bool IsSelected { get; set; }

        [JsonIgnore]
        public bool IsTokenValid => TokenExpiry.HasValue && TokenExpiry.Value > DateTime.UtcNow.AddMinutes(5);

        [JsonIgnore]
        public string AvatarDisplayUrl => AvatarUrl ?? GetDefaultAvatarUrl();

        private string GetDefaultAvatarUrl()
        {
            // Use a deterministic default avatar based on username
            int hash = Username.GetHashCode();
            bool isAlex = hash % 2 == 0;
            return isAlex 
                ? "https://minotar.net/avatar/Alex/100.png" 
                : "https://minotar.net/avatar/Steve/100.png";
        }
    }

    public class LauncherSettings
    {
        public const string DefaultJvmArguments = "-XX:+UseG1GC -XX:+ParallelRefProcEnabled -XX:MaxGCPauseMillis=50 -XX:+UnlockExperimentalVMOptions -XX:+DisableExplicitGC -XX:+AlwaysPreTouch -XX:G1NewSizePercent=30 -XX:G1MaxNewSizePercent=40 -XX:G1HeapRegionSize=8M -XX:G1ReservePercent=20 -XX:G1HeapWastePercent=5 -XX:G1MixedGCCountTarget=4 -XX:InitiatingHeapOccupancyPercent=15 -XX:G1MixedGCLiveThresholdPercent=90 -XX:G1RSetUpdatingPauseTimePercent=5 -XX:SurvivorRatio=32 -XX:+PerfDisableSharedMem -XX:MaxTenuringThreshold=1";

        [JsonProperty("ramMB")]
        public int RamMB { get; set; } = 4096;

        [JsonProperty("javaPath")]
        public string? JavaPath { get; set; }

        [JsonProperty("jvmArguments")]
        public string JvmArguments { get; set; } = DefaultJvmArguments;

        [JsonProperty("gameDirectory")]
        public string? GameDirectory { get; set; }

        [JsonProperty("windowWidth")]
        public int WindowWidth { get; set; } = 1280;

        [JsonProperty("windowHeight")]
        public int WindowHeight { get; set; } = 720;

        [JsonProperty("fullscreen")]
        public bool Fullscreen { get; set; } = false;

        [JsonProperty("autoCloseLauncher")]
        public bool AutoCloseLauncher { get; set; } = false;

        [JsonProperty("checkForUpdates")]
        public bool CheckForUpdates { get; set; } = true;

        /// <summary>
        /// When false (default), existing files are trusted and their SHA1 hash is
        /// never recomputed on launch. Combined with "only verify existence", this
        /// keeps the launch at ~0 network and minimal disk I/O.
        /// </summary>
        [JsonProperty("checkFileHash")]
        public bool CheckFileHash { get; set; } = false;

        /// <summary>
        /// Never download anything on launch; use local files as-is.
        /// </summary>
        [JsonProperty("offlineFirst")]
        public bool OfflineFirst { get; set; } = true;

        [JsonProperty("offlineMode")]
        public bool OfflineMode { get; set; } = false;

        [JsonProperty("priorityClass")]
        public ProcessPriorityClass PriorityClass { get; set; } = ProcessPriorityClass.High;

        [JsonProperty("theme")]
        public string Theme { get; set; } = "dark"; // dark | light

        [JsonProperty("language")]
        public string Language { get; set; } = "tr"; // tr | en

        [JsonProperty("curseForgeApiKey")]
        public string? CurseForgeApiKey { get; set; }

        /// <summary>
        /// Düşük Grafik: options.txt içinde graphics=FAST (yüksek FPS).
        /// </summary>
        [JsonProperty("lowGraphics")]
        public bool LowGraphics { get; set; } = false;

        public LauncherSettings Clone()
        {
            return new LauncherSettings
            {
                RamMB = RamMB,
                JavaPath = JavaPath,
                JvmArguments = JvmArguments,
                GameDirectory = GameDirectory,
                WindowWidth = WindowWidth,
                WindowHeight = WindowHeight,
                Fullscreen = Fullscreen,
                AutoCloseLauncher = AutoCloseLauncher,
                CheckForUpdates = CheckForUpdates,
                CheckFileHash = CheckFileHash,
                OfflineFirst = OfflineFirst,
                OfflineMode = OfflineMode,
                PriorityClass = PriorityClass,
                Theme = Theme,
                Language = Language,
                CurseForgeApiKey = CurseForgeApiKey,
                LowGraphics = LowGraphics
            };
        }
    }

    public class GameVersionInfo
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("type")]
        public string Type { get; set; } = "release";

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("version")]
        public string Version { get; set; } = string.Empty;

        [JsonProperty("releaseTime")]
        public DateTime ReleaseTime { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; } = string.Empty;

        [JsonProperty("isInstalled")]
        public bool IsInstalled { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }

        [JsonProperty("requiresJava")]
        public int RequiresJava { get; set; } = 17;

        [JsonProperty("mainClass")]
        public string? MainClass { get; set; }

        [JsonProperty("libraries")]
        public List<LibraryInfo> Libraries { get; set; } = new();

        [JsonProperty("assetsIndex")]
        public AssetIndexInfo? AssetsIndex { get; set; }

        [JsonProperty("status")]
        public VersionStatus Status { get; set; } = VersionStatus.Unknown;
    }

    public class LibraryInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("url")]
        public string? Url { get; set; }

        [JsonProperty("path")]
        public string? Path { get; set; }

        [JsonProperty("downloads")]
        public LibraryDownloads? Downloads { get; set; }

        [JsonProperty("natives")]
        public Dictionary<string, string>? Natives { get; set; }

        [JsonProperty("rules")]
        public List<LibraryRule>? Rules { get; set; }
    }

    public class LibraryDownloads
    {
        [JsonProperty("artifact")]
        public ArtifactInfo? Artifact { get; set; }

        [JsonProperty("classifiers")]
        public Dictionary<string, ArtifactInfo>? Classifiers { get; set; }
    }

    public class ArtifactInfo
    {
        [JsonProperty("path")]
        public string? Path { get; set; }

        [JsonProperty("url")]
        public string? Url { get; set; }

        [JsonProperty("sha1")]
        public string? Sha1 { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }
    }

    public class LibraryRule
    {
        [JsonProperty("action")]
        public string Action { get; set; } = "allow";

        [JsonProperty("os")]
        public OsRule? Os { get; set; }

        [JsonProperty("features")]
        public Dictionary<string, bool>? Features { get; set; }
    }

    public class OsRule
    {
        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("version")]
        public string? Version { get; set; }

        [JsonProperty("arch")]
        public string? Arch { get; set; }
    }

    public class AssetIndexInfo
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("url")]
        public string Url { get; set; } = string.Empty;

        [JsonProperty("size")]
        public long Size { get; set; }

        [JsonProperty("sha1")]
        public string Sha1 { get; set; } = string.Empty;

        [JsonProperty("totalSize")]
        public long TotalSize { get; set; }
    }

    public class VersionManifest
    {
        [JsonProperty("latest")]
        public LatestVersions Latest { get; set; } = new();

        [JsonProperty("versions")]
        public List<VersionEntry> Versions { get; set; } = new();
    }

    public class LatestVersions
    {
        [JsonProperty("release")]
        public string Release { get; set; } = string.Empty;

        [JsonProperty("snapshot")]
        public string Snapshot { get; set; } = string.Empty;
    }

    public class VersionEntry
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("type")]
        public string Type { get; set; } = "release";

        [JsonProperty("url")]
        public string Url { get; set; } = string.Empty;

        [JsonProperty("time")]
        public DateTime Time { get; set; }

        [JsonProperty("releaseTime")]
        public DateTime ReleaseTime { get; set; }
    }

    public class MinecraftVersionJson
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("inheritsFrom")]
        public string? InheritsFrom { get; set; }

        [JsonProperty("minecraftArguments")]
        public string? MinecraftArguments { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("mainClass")]
        public string MainClass { get; set; } = string.Empty;

        [JsonProperty("minimumLauncherVersion")]
        public int MinimumLauncherVersion { get; set; }

        [JsonProperty("assets")]
        public string Assets { get; set; } = string.Empty;

        [JsonProperty("assetIndex")]
        public AssetIndexInfo AssetIndex { get; set; } = new();

        [JsonProperty("libraries")]
        public List<LibraryInfo> Libraries { get; set; } = new();

        [JsonProperty("arguments")]
        public LaunchArguments Arguments { get; set; } = new();

        [JsonProperty("javaVersion")]
        public JavaVersionInfo? JavaVersion { get; set; }
    }

    public class LaunchArguments
    {
        [JsonProperty("game")]
        public List<object> Game { get; set; } = new();

        [JsonProperty("jvm")]
        public List<object> Jvm { get; set; } = new();
    }

    public class JavaVersionInfo
    {
        [JsonProperty("component")]
        public string Component { get; set; } = string.Empty;

        [JsonProperty("majorVersion")]
        public int MajorVersion { get; set; }
    }

    public class MicrosoftAuthResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; } = string.Empty;

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; } = string.Empty;

        [JsonProperty("scope")]
        public string Scope { get; set; } = string.Empty;
    }

    public class MicrosoftProfile
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("skins")]
        public List<SkinInfo> Skins { get; set; } = new();

        [JsonProperty("capes")]
        public List<CapeInfo> Capes { get; set; } = new();
    }

    public class SkinInfo
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("state")]
        public string State { get; set; } = string.Empty;

        [JsonProperty("url")]
        public string Url { get; set; } = string.Empty;

        [JsonProperty("variant")]
        public string Variant { get; set; } = string.Empty;
    }

    public class CapeInfo
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("state")]
        public string State { get; set; } = string.Empty;

        [JsonProperty("url")]
        public string Url { get; set; } = string.Empty;
    }

    public enum AuthType
    {
        Offline,
        Microsoft
    }

    public enum VersionStatus
    {
        Unknown,
        Installed,
        Available,
        Outdated,
        Corrupted
    }
}