﻿using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TMPro;
using ToolModData;
using UnityEngine;
using static ToolModBepInEx.PatchMgr;

namespace ToolModBepInEx
{
    [HarmonyPatch(typeof(NoticeMenu), "Awake")]
    public static class Help_Patch
    {
        public static void Postfix() => Core.Instance.Value.LateInit();
    }

    [BepInPlugin("inf75.toolmod", "ToolMod", "3.23")]
    public class Core : BasePlugin
    {
        public void LateInit()
        {
            try
            {
                if (Port.Value.Value < 10000 || Port.Value.Value > 60000)
                {
                    MessageBox(0, "Port值无效，已使用默认值13531", "修改器警告", 0);
                    Port.Value.Value = 13531;
                }

                bool needRegen = false;
                string hash = Utils.ComputeFolderHash(Paths.PluginPath);
                if (ModsHash.Value.Value != hash)
                {
                    needRegen = true;
                    ModsHash.Value.Value = hash;

                    if (Directory.Exists("PVZRHTools\\GardenTools\\res"))
                    {
                        foreach (var f in Directory.GetFiles("PVZRHTools\\GardenTools\\res\\0"))
                        {
                            if (!(f.EndsWith("-1.png") || f.EndsWith("error.png")))
                            {
                                File.Delete(f);
                            }
                        }
                        foreach (var f in Directory.GetFiles("PVZRHTools\\GardenTools\\res\\1"))
                        {
                            if (!(f.EndsWith("-1.png") || f.EndsWith("error.png")))
                            {
                                File.Delete(f);
                            }
                        }
                        foreach (var f in Directory.GetFiles("PVZRHTools\\GardenTools\\res\\2"))
                        {
                            if (!(f.EndsWith("-1.png") || f.EndsWith("error.png")))
                            {
                                File.Delete(f);
                            }
                        }
                    }
                }

                MLogger.LogWarning("以下id信息为动态生成，仅适用于当前游戏实例！！！");
                MLogger.LogWarning("以下id信息为动态生成，仅适用于当前游戏实例！！！");
                MLogger.LogWarning("以下id信息为动态生成，仅适用于当前游戏实例！！！");

                Dictionary<int, string> plants = [];
                Dictionary<int, string> zombies = [];
                GameObject gameObject = new();
                GameObject back1 = new();
                back1.transform.SetParent(gameObject.transform);
                GameObject name1 = new("Name");
                GameObject shadow1 = new("Shadow");
                shadow1.transform.SetParent(name1.transform);
                var nameText1 = name1.AddComponent<TextMeshPro>();
                name1.transform.SetParent(gameObject.transform);
                GameObject info1 = new("Info");
                info1.transform.SetParent(gameObject.transform);
                GameObject cost1 = new("Cost");
                cost1.transform.SetParent(gameObject.transform);
                var alm = gameObject.AddComponent<AlmanacPlantBank>();
                alm.cost = cost1.AddComponent<TextMeshPro>();
                alm.plantName_shadow = shadow1.AddComponent<TextMeshPro>();
                alm.plantName = name1.GetComponent<TextMeshPro>();
                alm.introduce = info1.AddComponent<TextMeshPro>();
                gameObject.AddComponent<TravelMgr>();
                string gardenIds = "";

                for (int i = 0; i < GameAPP.resourcesManager.allPlants.Count; i++)
                {
                    alm.theSeedType = (int)GameAPP.resourcesManager.allPlants[i];
                    alm.InitNameAndInfoFromJson();
                    string item = $"{(int)GameAPP.resourcesManager.allPlants[i]} : {alm.plantName.GetComponent<TextMeshPro>().text}";
                    MLogger.LogInfo($"Dumping Plant String: {item}");
                    plants.Add((int)GameAPP.resourcesManager.allPlants[i], item);
                    HealthPlants.Add(GameAPP.resourcesManager.allPlants[i], -1);
                    if (needRegen)
                    {
                        gardenIds = Utils.OutputGardenTexture(i, alm.plantName.GetComponent<TextMeshPro>().text, gardenIds);
                    }

                    alm.plantName.GetComponent<TextMeshPro>().text = "";
                }
                UnityEngine.Object.Destroy(gameObject);
                if (needRegen)
                {
                    if (File.Exists("PVZRHTools/GardenTools/plant_id.txt"))
                    {
                        File.Delete("PVZRHTools/GardenTools/plant_id.txt");
                    }
                    using FileStream gid = new("PVZRHTools/GardenTools/plant_id.txt", FileMode.Create);
                    byte[] buffer = Encoding.UTF8.GetBytes(gardenIds);
                    gid.Write(buffer, 0, buffer.Length);
                    gid.Flush();
                    Utils.GenerateGardenData();
                }

                GameObject gameObject2 = new();
                GameObject back2 = new();
                back2.transform.SetParent(gameObject2.transform);
                back2.AddComponent<SpriteRenderer>();
                GameObject name2 = new("Name");
                GameObject shadow2 = new("Name_1");
                shadow2.transform.SetParent(name2.transform);
                shadow2.AddComponent<TextMeshPro>();
                name2.AddComponent<TextMeshPro>();
                name2.transform.SetParent(gameObject2.transform);
                name2.AddComponent<TextMeshPro>();
                GameObject info2 = new("Info");
                info2.transform.SetParent(gameObject2.transform);
                var almz = gameObject2.AddComponent<AlmanacMgrZombie>();
                almz.info = info2;
                almz.zombieName = name2;
                almz.introduce = info2.AddComponent<TextMeshPro>(); ;

                for (int i = 0; i < GameAPP.resourcesManager.allZombieTypes.Count; i++)
                {
                    almz.theZombieType = GameAPP.resourcesManager.allZombieTypes[i];
                    almz.InitNameAndInfoFromJson();
                    HealthZombies.Add(GameAPP.resourcesManager.allZombieTypes[i], -1);

                    if (!string.IsNullOrEmpty(almz.zombieName.GetComponent<TextMeshPro>().text))
                    {
                        string item = $"{(int)GameAPP.resourcesManager.allZombieTypes[i]} : {almz.zombieName.GetComponent<TextMeshPro>().text}";
                        MLogger.LogInfo($"Dumping Zombie String: {item}");
                        zombies.Add((int)GameAPP.resourcesManager.allZombieTypes[i], item);
                        almz.zombieName.GetComponent<TextMeshPro>().text = "";
                    }
                }
                UnityEngine.Object.Destroy(gameObject2);
                zombies.Add(54, "54 : 试验假人僵尸");

                List<string> advBuffs = [];
                for (int i = 0; i < TravelMgr.advancedBuffs.Count; i++)
                {
                    if (TravelMgr.advancedBuffs[i] is not null)
                    {
                        MLogger.LogInfo($"Dumping Advanced Buff String:#{i} {TravelMgr.advancedBuffs[i]}");
                        advBuffs.Add($"#{i} {TravelMgr.advancedBuffs[i]}");
                    }
                }
                List<string> ultiBuffs = [];
                for (int i = 0; i < TravelMgr.ultimateBuffs.Count; i++)
                {
                    if (TravelMgr.ultimateBuffs[i] is not null)
                    {
                        MLogger.LogInfo($"Dumping Ultimate Buff String:#{i} {TravelMgr.ultimateBuffs[i]}");
                        ultiBuffs.Add($"#{i} {TravelMgr.ultimateBuffs[i]}");
                    }
                }
                List<string> debuffs = [];
                for (int i = 0; i < TravelMgr.debuffs.Count; i++)
                {
                    if (TravelMgr.debuffs[i] is not null)
                    {
                        MLogger.LogInfo($"Dumping Debuff String:#{i} {TravelMgr.debuffs[i]}");
                        debuffs.Add(TravelMgr.debuffs[i]);
                    }
                }
                AdvBuffs = new bool[TravelMgr.advancedBuffs.Count];
                PatchMgr.UltiBuffs = new bool[TravelMgr.ultimateBuffs.Count];
                Debuffs = new bool[TravelMgr.debuffs.Count];

                Dictionary<int, string> bullets = [];

                for (int i = 0; i < GameAPP.resourcesManager.allBullets.Count; i++)
                {
                    if (GameAPP.resourcesManager.bulletPrefabs[GameAPP.resourcesManager.allBullets[i]] is not null)
                    {
                        string text = $"{(int)GameAPP.resourcesManager.allBullets[i]} : {GameAPP.resourcesManager.bulletPrefabs[GameAPP.resourcesManager.allBullets[i]].name}";
                        MLogger.LogInfo($"Dumping Bullet String: {text}");
                        bullets.Add((int)GameAPP.resourcesManager.allBullets[i], text);
                        BulletDamage.Add(GameAPP.resourcesManager.allBullets[i], -1);
                    }
                }
                Dictionary<int, string> firsts = [];
                foreach (var first in Enum.GetValues(typeof(Zombie.FirstArmorType)))
                {
                    firsts.Add((int)first, $"{first}");
                }
                Dictionary<int, string> seconds = [];
                foreach (var second in Enum.GetValues(typeof(Zombie.SecondArmorType)))
                {
                    seconds.Add((int)second, $"{second}");
                }
                MLogger.LogWarning("以上id信息为动态生成，仅适用于当前游戏实例！！！");
                MLogger.LogWarning("以上id信息为动态生成，仅适用于当前游戏实例！！！");
                MLogger.LogWarning("以上id信息为动态生成，仅适用于当前游戏实例！！！");

                InitData data = new()
                {
                    Plants = plants,
                    Zombies = zombies,
                    AdvBuffs = [.. advBuffs],
                    UltiBuffs = [.. ultiBuffs],
                    Bullets = bullets,
                    FirstArmors = firsts,
                    SecondArmors = seconds,
                    Debuffs = [.. debuffs],
                };
                File.WriteAllText("./PVZRHTools/InitData.json", JsonSerializer.Serialize(data));
                _ = DataSync.Instance.Value;
            }
            catch (Exception ex)
            {
                LoggerInstance.LogError(ex);
            }
            inited = true;
        }

        public override void Load()
        {
            Console.OutputEncoding = Encoding.UTF8;
            ClassInjector.RegisterTypeInIl2Cpp<PatchMgr>();
            ClassInjector.RegisterTypeInIl2Cpp<DataProcessor>();
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            Instance = new(this);
            if (Time.timeScale == 0) Time.timeScale = 1;
            SyncSpeed = Time.timeScale;
            Port = new(Config.Bind("PVZRHTools", "Port", 13531, "修改窗口无法出现时可尝试修改此数值，范围10000~60000"));
            AlmanacZombieMindCtrl = new(Config.Bind("PVZRHTools", nameof(AlmanacZombieMindCtrl), false));
            KeyTimeStop = new(Config.Bind("PVZRHTools", nameof(KeyTimeStop), KeyCode.Alpha5));
            KeyShowGameInfo = new(Config.Bind("PVZRHTools", nameof(KeyShowGameInfo), KeyCode.BackQuote));
            KeyAlmanacCreatePlant = new(Config.Bind("PVZRHTools", nameof(KeyAlmanacCreatePlant), KeyCode.B));
            KeyAlmanacCreateZombie = new(Config.Bind("PVZRHTools", nameof(KeyAlmanacCreateZombie), KeyCode.N));
            KeyAlmanacZombieMindCtrl = new(Config.Bind("PVZRHTools", nameof(KeyAlmanacZombieMindCtrl), KeyCode.LeftControl));
            KeyTopMostCardBank = new(Config.Bind("PVZRHTools", nameof(KeyTopMostCardBank), KeyCode.Tab));
            KeyAlmanacCreatePlantVase = new(Config.Bind("PVZRHTools", nameof(KeyAlmanacCreatePlantVase), KeyCode.J));
            KeyAlmanacCreateZombieVase = new(Config.Bind("PVZRHTools", nameof(KeyAlmanacCreateZombieVase), KeyCode.K));
            ModsHash = new(Config.Bind("PVZRHTools", nameof(ModsHash), ""));

            KeyBindings = new
            ([
                KeyTimeStop.Value,KeyTopMostCardBank.Value,KeyShowGameInfo.Value,
                KeyAlmanacCreatePlant.Value,KeyAlmanacCreateZombie.Value,KeyAlmanacZombieMindCtrl.Value,
                KeyAlmanacCreatePlantVase.Value,KeyAlmanacCreateZombieVase.Value
            ]);
            Config.Save();
        }

        public override bool Unload()
        {
            if (inited)
            {
                if (GameAPP.gameSpeed == 0) GameAPP.gameSpeed = 1;
                DataSync.Instance.Value.SendData(new Exit());
                Thread.Sleep(100);
                DataSync.Instance.Value.modifierSocket.Shutdown(SocketShutdown.Both);
                DataSync.Instance.Value.modifierSocket.Close();
            }
            inited = false;
            return true;
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr MessageBox(int hWnd, string text, string caption, uint type);

        public static Lazy<ConfigEntry<bool>> AlmanacZombieMindCtrl { get; set; } = new();
        public static Lazy<Core> Instance { get; set; } = new();
        public static Lazy<ConfigEntry<KeyCode>> KeyAlmanacCreatePlant { get; set; } = new();
        public static Lazy<ConfigEntry<KeyCode>> KeyAlmanacCreatePlantVase { get; set; } = new();
        public static Lazy<ConfigEntry<KeyCode>> KeyAlmanacCreateZombie { get; set; } = new();
        public static Lazy<ConfigEntry<KeyCode>> KeyAlmanacCreateZombieVase { get; set; } = new();
        public static Lazy<ConfigEntry<KeyCode>> KeyAlmanacZombieMindCtrl { get; set; } = new();
        public static Lazy<List<ConfigEntry<KeyCode>>> KeyBindings { get; set; } = new();
        public static Lazy<ConfigEntry<KeyCode>> KeyShowGameInfo { get; set; } = new();
        public static Lazy<ConfigEntry<KeyCode>> KeyTimeStop { get; set; } = new();
        public static Lazy<ConfigEntry<KeyCode>> KeyTopMostCardBank { get; set; } = new();
        public static Lazy<ConfigEntry<string>> ModsHash { get; set; } = new();

        public static Lazy<ConfigEntry<int>> Port { get; set; } = new();
        public ManualLogSource LoggerInstance => BepInEx.Logging.Logger.CreateLogSource("ToolMod");
        public static bool inited = false;
    }

    public class Utils
    {
        public static string ComputeFolderHash(string folderPath)
        {
            using SHA256 sha256 = SHA256.Create();
            sha256.Initialize();
            ProcessDirectory(folderPath, sha256);
            sha256.TransformFinalBlock([], 0, 0);
            return BytesToHex(sha256.Hash!);
        }

        public static Texture2D ConvertViaRenderTexture(Texture2D source)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary(
                source.width,
                source.height,
                0,
                RenderTextureFormat.ARGB32  // 指定兼容格式
            );

            // 将原纹理复制到RenderTexture
            UnityEngine.Graphics.Blit(source, renderTex);

            // 从RenderTexture读取像素
            RenderTexture.active = renderTex;
            Texture2D result = new(source.width, source.height, TextureFormat.RGBA32, false);
            result.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            result.Apply();

            // 清理资源
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(renderTex);
            return result;
        }

        public static Texture2D ExtractSpriteTexture(Sprite sprite)
        {
            // 获取Sprite在图集中的纹理区域
            Rect rect = sprite.textureRect;

            // 创建新Texture2D
            Texture2D outputTex = new(
                (int)rect.width,
                (int)rect.height,
                TextureFormat.RGBA32,
                false
            );

            // 复制像素
            UnityEngine.Color[] pixels = ConvertViaRenderTexture(sprite.texture).GetPixels(
                (int)rect.x,
                (int)rect.y,
                (int)rect.width,
                (int)rect.height
            );

            outputTex.SetPixels(pixels);
            outputTex.Apply();

            return outputTex;
        }

        public static void GenerateGardenData()
        {
            var plantids = GameAPP.resourcesManager.allPlants;
            int currentPage = -1;
            List<Dictionary<string, object>> currentPlants = [];

            for (int i = 0; i < plantids.Count; i++)
            {
                int page = i / 32;
                int row = (i % 32) / 8;
                int column = i % 8;
                int plantType = (int)plantids[i];

                var plantDict = new Dictionary<string, object>
            {
                { "thePlantRow", row },
                { "thePlantColumn", column },
                { "thePlantType", plantType },
                { "growStage", 2 },
                { "waterLevel", 100 },
                { "love", 100 },
                { "nextTime", 11451419198L },
                { "needTool", 1 },
                { "page", page },
                { "GrowStage", 2 }
            };

                if (page != currentPage)
                {
                    if (currentPage != -1)
                    {
                        WritePage(currentPlants, currentPage);
                    }
                    currentPage = page;
                    currentPlants.Clear();
                }

                currentPlants.Add(plantDict);

                // 处理最后一页
                if (i == plantids.Count - 1)
                {
                    WritePage(currentPlants, currentPage);
                }
            }
        }

        public static bool LoadPlantData()
        {
            try
            {// 加载文本资源
                string text = "";
                if (File.Exists("PVZRHTools\\plant_data.csv"))
                {
                    text = new StreamReader(File.Open("PVZRHTools\\plant_data.csv", FileMode.Open, FileAccess.Read, FileShare.ReadWrite)).ReadToEnd();
                }
                else
                {
                    TextAsset t = Resources.Load<TextAsset>("plant_data");
                    if (t is not null)
                    {
                        using FileStream f = File.Create("PVZRHTools/plant_data.csv");

                        byte[] buffer = t.bytes;
                        f.Write(buffer, 0, buffer.Length);
                        f.Flush();
                    }
                    return true;
                }

                // 分割文本行
                string[] lines = text.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);

                for (int i = 1; i < lines.Length; i++) // 跳过标题行
                {
                    string line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line))
                        continue;

                    string[] fields = line.Split(',');
                    if (fields.Length < 7)
                    {
                        MLogger.LogError($"Invalid plant data format at line {i + 1}");
                        continue;
                    }

                    try
                    {
                        if (PlantDataLoader.plantDatas.ContainsKey((PlantType)int.Parse(fields[0])))
                        {
                            PlantDataLoader.plantDatas[(PlantType)int.Parse(fields[0])].field_Public_Single_0 = float.Parse(fields[1]);
                            PlantDataLoader.plantDatas[(PlantType)int.Parse(fields[0])].field_Public_Single_1 = float.Parse(fields[2]);
                            PlantDataLoader.plantDatas[(PlantType)int.Parse(fields[0])].attackDamage = int.Parse(fields[3]);
                            PlantDataLoader.plantDatas[(PlantType)int.Parse(fields[0])].field_Public_Int32_0 = int.Parse(fields[4]);
                            PlantDataLoader.plantDatas[(PlantType)int.Parse(fields[0])].field_Public_Single_2 = float.Parse(fields[5]);
                            PlantDataLoader.plantDatas[(PlantType)int.Parse(fields[0])].field_Public_Int32_1 = int.Parse(fields[6]);
                            if (int.Parse(fields[0]) < PlantDataLoader.plantData.Count)
                                PlantDataLoader.plantData[int.Parse(fields[0])] = PlantDataLoader.plantDatas[(PlantType)int.Parse(fields[0])];
                        }
                    }
                    catch (FormatException ex)
                    {
                        MLogger.LogError($"Error parsing data at line {i + 1}: {ex.Message}");
                    }
                }
                return true;
            }
            catch (FileNotFoundException e1)
            {
                MLogger.LogError(e1);
            }
            catch (IOException e1)
            {
                MLogger.LogError(e1);
                MLogger.LogError("plant_data.csv被占用");
            }

            return false;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:验证平台兼容性", Justification = "<挂起>")]
        public static string OutputGardenTexture(int i, string name, string gardenIds)
        {
            string outputBaseDir = @"PVZRHTools\GardenTools\res";
            string filename = ((int)GameAPP.resourcesManager.allPlants[i]).ToString() + ".png";
            int[] sizes = [30, 45, 60];
            try
            {
                Sprite sprite = GameAPP.resourcesManager.plantPreviews[GameAPP.resourcesManager.allPlants[i]].GetComponent<SpriteRenderer>().sprite;

                using Image originalImage = Image.FromStream(
                    new MemoryStream([.. ImageConversion.EncodeToPNG(ConvertViaRenderTexture(ExtractSpriteTexture(sprite)))]));
                for (int ii = 0; ii < sizes.Length; ii++)
                {
                    int size = sizes[ii];
                    string outputDir = Path.Combine(outputBaseDir, $"{ii}");

                    // 确保输出目录存在
                    Directory.CreateDirectory(outputDir);

                    // 调整尺寸
                    using Bitmap resizedImage = new(size, size);
                    using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(resizedImage))
                    {
                        // 设置高质量缩放参数
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        g.CompositingQuality = CompositingQuality.HighQuality;

                        g.DrawImage(originalImage, 0, 0, size, size);
                    }

                    // 保存图片
                    string outputPath = Path.Combine(outputDir, filename);
                    resizedImage.Save(outputPath, System.Drawing.Imaging.ImageFormat.Png);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"处理 {filename} 时出错: {ex}");
            }
            return string.Concat(gardenIds.Concat($"ID: {(int)GameAPP.resourcesManager.allPlants[i]}, {name}\n"));
        }

        private static string BytesToHex(byte[] bytes)
        {
            StringBuilder hex = new(bytes.Length * 2);
            foreach (byte b in bytes)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        private static string GetRelativePath(string rootDir, string fullPath)
        {
            // 确保根目录以分隔符结尾
            if (!rootDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
                rootDir += Path.DirectorySeparatorChar;

            Uri rootUri = new(rootDir);
            Uri fullUri = new(fullPath);
            string relativePath = Uri.UnescapeDataString(rootUri.MakeRelativeUri(fullUri).ToString());

            // 统一替换路径分隔符为当前系统分隔符
            relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

            return relativePath;
        }

        private static void ProcessDirectory(string rootDir, SHA256 sha256)
        {
            // 处理当前目录下的所有文件，按相对路径排序
            var files = Directory.GetFiles(rootDir)
                .Select(f => new { FullPath = f, RelativePath = GetRelativePath(rootDir, f) })
                .OrderBy(f => f.RelativePath, StringComparer.Ordinal);

            foreach (var file in files)
            {
                if (file.RelativePath.Contains("ToolMod") || file.RelativePath.Contains("CustomizeLib") || file.FullPath.Contains("ModifiedPlus") || file.FullPath.Contains("UnityExplorer"))
                    continue;
                // 将相对路径作为元数据添加到哈希
                byte[] pathBytes = Encoding.UTF8.GetBytes(file.RelativePath);
                sha256.TransformBlock(pathBytes, 0, pathBytes.Length, null, 0);

                // 将文件内容添加到哈希
                using FileStream stream = File.OpenRead(file.FullPath);
                byte[] buffer = new byte[8192];
                int bytesRead;
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    sha256.TransformBlock(buffer, 0, bytesRead, null, 0);
                }
            }
        }

        private static void WritePage(List<Dictionary<string, object>> plants, int page)
        {
            var jsonData = new
            {
                plantData = plants
            };

            string jsonString = JsonSerializer.Serialize(jsonData);

            // 自动处理数组末尾逗号问题
            string fileName = page == 0 ? "PVZRHTools\\GardenTools\\gar_all\\GardenData.json" : $"PVZRHTools\\GardenTools\\gar_all\\GardenData{page}.json";
            fileName = Path.Combine(Paths.GameRootPath, fileName);
            // 获取桌面路径示例（可根据需要修改）
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string fullPath = Path.Combine(desktopPath, fileName);

            using StreamWriter file = File.CreateText(fullPath);
            file.Write(jsonString);
        }
    }
}