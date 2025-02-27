﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityModManagerNet;
using XLObjectDropper.Utilities.Save;
using XLObjectDropper.Utilities.Save.Legacy;

namespace XLObjectDropper.Utilities
{
	public class SaveManager
	{
		private static SaveManager __instance;
		public static SaveManager Instance => __instance ?? (__instance = new SaveManager());

		public UnityModManager.ModEntry ModEntry;

		public static string SaveDir => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "SkaterXL", "XLObjectDropper", "Saves");

		public List<LevelSaveData> LoadedSaves;

		//TODO: Actually implement this
		public bool HasUnsavedChanges = true;

		public void SaveCurrentSpawnables(string fileName)
		{
			try
			{
				var levelConfigToSave = new LevelSaveData
				{
					levelHash = LevelManager.Instance.currentLevel.hash,
					levelName = LevelManager.Instance.currentLevel.name,
					dateModified = DateTime.Now,
				};

				var spawnedItems = SpawnableManager.SpawnedObjects;

				if (spawnedItems == null || !spawnedItems.Any()) return;

				foreach (var spawnable in spawnedItems)
				{
					var instance = spawnable.SpawnedInstance;

					var objectSaveData = new GameObjectSaveData
					{
						Id = instance.name,
						bundleName = spawnable.BundleName,
						position = new SerializableVector3(instance.transform.position),
						rotation = new SerializableQuaternion(instance.transform.rotation),
						localScale = new SerializableVector3(instance.transform.localScale)
					};

					foreach (var settings in spawnable.Settings)
					{
						var settingsSaveData = settings.ConvertToSaveSettings();
						if (settingsSaveData != null)
						{
							objectSaveData.settings.Add(settingsSaveData);
						}
					}

					levelConfigToSave.gameObjects.Add(objectSaveData);
				}

				string json = JsonConvert.SerializeObject(levelConfigToSave, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });

				var currentSaveDir = Path.Combine(SaveDir, levelConfigToSave.levelName);

				if (!Directory.Exists(currentSaveDir))
				{
					Directory.CreateDirectory(currentSaveDir);
				}

				File.WriteAllText(Path.Combine(currentSaveDir, $"{fileName}.json"), json);
			}
			catch (Exception e)
			{
				UnityModManager.Logger.Log("XLObjectDropper.  Error occurred while saving: " + e.Message);
				throw;
			}
		}

		public void LoadAllSaves()
		{
			if (!Directory.Exists(SaveDir))
			{
				Directory.CreateDirectory(SaveDir);
				return;
			}

			if (LoadedSaves == null)
			{
				LoadedSaves = new List<LevelSaveData>();
			}
			else
			{
				LoadedSaves.Clear();
			}

			var saveFiles = Directory.GetFiles(SaveDir, "*.json", SearchOption.AllDirectories);

			if (saveFiles.Any())
			{
				foreach (var saveFile in saveFiles)
				{
					var content = File.ReadAllText(saveFile);

					if (string.IsNullOrEmpty(content))
						continue;

					try
					{
						var loadedLevelSave = JsonConvert.DeserializeObject<LevelSaveData>(content, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
						loadedLevelSave.filePath = saveFile;
						loadedLevelSave.fileName = Path.GetFileNameWithoutExtension(saveFile);

						LoadedSaves.Add(loadedLevelSave);
						continue;
					}
					catch (Exception ex)
					{
						UnityModManager.Logger.Log($"XLObjectDropper: Unable to deserialize {saveFile}.  Will attempt legacy deserialization.");
					}

					try
					{
						var loadedLegacySave = JsonConvert.DeserializeObject<List<LegacyGameObjectSaveData>>(content);

						var loadedLevelSave = new LevelSaveData();
						loadedLevelSave.isLegacy = true;
						loadedLevelSave.filePath = saveFile;
						loadedLevelSave.fileName = Path.GetFileNameWithoutExtension(saveFile);

						foreach (var gameObject in loadedLegacySave)
						{
							var saveData = new GameObjectSaveData
							{
								Id = gameObject.objectName,
								position = new SerializableVector3(gameObject.posX, gameObject.posY, gameObject.posZ),
								localScale = new SerializableVector3(gameObject.scaleX, gameObject.scaleY, gameObject.scaleZ)
							};

							var tmpQuat = Quaternion.Euler(gameObject.rotX, gameObject.rotY, gameObject.rotZ);
							saveData.rotation = new SerializableQuaternion(tmpQuat.x, tmpQuat.y, tmpQuat.z, tmpQuat.w);

							loadedLevelSave.gameObjects.Add(saveData);
						}

						LoadedSaves.Add(loadedLevelSave);
					}
					catch (Exception ex)
					{
						UnityModManager.Logger.Log($"XLObjectDropper: Unable to deserialize {saveFile} using legacy deserialization.");
					}
				}
			}
		}

		public List<LevelSaveData> GetLoadedSavesByLevelHash(string hash)
		{
			if (LoadedSaves == null || !LoadedSaves.Any())
			{
				return new List<LevelSaveData>();
			}

			return LoadedSaves.Where(x => x.levelHash == hash).ToList();
		}

		public List<LevelSaveData> GetLoadedLegacySaves(string name)
		{
			if (LoadedSaves == null || !LoadedSaves.Any())
			{
				return new List<LevelSaveData>();
			}

			return LoadedSaves.Where(x => x.isLegacy).ToList();
		}

		public List<LevelSaveData> GetLoadedSavesByLevelName(string name)
		{
			if (LoadedSaves == null || !LoadedSaves.Any())
			{
				return new List<LevelSaveData>();
			}

			return LoadedSaves.Where(x => x.levelName == name).ToList();
		}
	}
}
