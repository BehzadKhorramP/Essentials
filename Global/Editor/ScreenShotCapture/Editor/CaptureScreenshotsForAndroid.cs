using System;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace MadApperEditor.Screenshot
{

    public class CaptureScreenshotsForAndroid : Editor
    {
        private const string DirectoryName = "Screenshots";
        private const string Prefix = "SCREENSHOTS_";
        private static readonly GameViewSize[] customSizes = {
        new(1242, 2688, "iPhone 6.5"),
        new(2048, 2732, "iPad Pro"),
        new(1800, 3200, "9_16")
    };
        private static int screenCount;
        private static int currentTask;

        [MenuItem("MAD/Screenshots/CaptureScreenshot #b", false, 560)]
        private static void CaptureScreenshot1()
        {
            if (screenCount == 0)
            {
                EditorCoroutine.Start(CaptureScreenshot());
            }
            else
            {
                Debug.LogError("Slowly buddy");
            }
        }

        private static IEnumerator CaptureScreenshot()
        {
            Time.timeScale = 0;

            EnsureDirectoryExists();
            IncrementScreenshotIndex();

            GameViewSizeGroupType groupType = GetCurrentSizeGroupType();
            Type gameViewType = GetGameViewType();
            EditorWindow gameView = EditorWindow.GetWindow(gameViewType);

            int originalSizeIndex = GetOriginalSizeIndex(gameView, gameViewType);

            foreach (GameViewSize customSize in customSizes)
            {
                screenCount++;
                currentTask++;
                ProcessCustomSize(customSize, groupType, gameView);
                while (currentTask > 0)
                {
                    yield return new WaitForEndOfFrame();
                }
            }
            while (screenCount > 0)
            {
                yield return new WaitForEndOfFrame();
            }
            Debug.Log("Screenshot complete");
            ResetGameViewSize(gameView, gameViewType, originalSizeIndex);

            Time.timeScale = 1;

        }

        private static void ProcessCustomSize(GameViewSize customSize, GameViewSizeGroupType groupType, EditorWindow gameView)
        {
            if (!GameViewSizeHelper.Contains(groupType, customSize))
            {
                GameViewSizeHelper.AddCustomSize(groupType, customSize);
            }
            GameViewSizeHelper.ChangeGameViewSize(groupType, customSize);

            EditorCoroutine.Start(CaptureAndSaveScreenshot(customSize, gameView, gameView.GetType()));
        }

        private static IEnumerator CaptureAndSaveScreenshot(GameViewSize customSize, EditorWindow gameView, Type gameViewType)
        {
            yield return new WaitForEndOfFrame();

            string filename = Path.Combine(DirectoryName, $"{customSize.baseText}_{GetIndexScreenshot()}.png");
            ScreenCapture.CaptureScreenshot(filename);
            while (File.Exists(filename) == false)
            {
                yield return new WaitForEndOfFrame();
            }

            gameView.Repaint();
            currentTask--;
            screenCount--;
        }

        private static void EnsureDirectoryExists()
        {
            if (!Directory.Exists(DirectoryName))
            {
                Directory.CreateDirectory(DirectoryName);
            }
        }

        private static void IncrementScreenshotIndex()
        {
            SetNumberScreenshots(GetIndexScreenshot() + 1);
        }

        public static void SetNumberScreenshots(int value)
        {
            PlayerPrefs.SetInt(Prefix, value);
        }

        public static int GetIndexScreenshot()
        {
            return PlayerPrefs.GetInt(Prefix, 0);
        }

        private static GameViewSizeGroupType GetCurrentSizeGroupType()
        {
            Type gameView = GetGameViewType();
            PropertyInfo currentSizeGroupType = gameView.GetProperty("currentSizeGroupType", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);
            return (GameViewSizeGroupType)currentSizeGroupType.GetValue(EditorWindow.GetWindow(gameView), null);
        }

        private static Type GetGameViewType()
        {
            Assembly assembly = typeof(EditorWindow).Assembly;
            return assembly.GetType("UnityEditor.GameView");
        }
        private static Vector2 GetCurrentGameViewSize()
        {
            Type gameViewType = GetGameViewType();
            EditorWindow gameView = EditorWindow.GetWindow(gameViewType);

            MethodInfo GetSizeOfMainGameView = gameViewType.GetMethod("GetSizeOfMainGameView", BindingFlags.NonPublic | BindingFlags.Static);
            object res = GetSizeOfMainGameView.Invoke(null, null);
            return (Vector2)res;
        }

        private static int GetOriginalSizeIndex(EditorWindow gameView, Type gameViewType)
        {
            PropertyInfo sizeIndexProp = gameViewType.GetProperty("selectedSizeIndex", BindingFlags.Instance | BindingFlags.Public);
            return (int)sizeIndexProp.GetValue(gameView, null);
        }

        private static void ResetGameViewSize(EditorWindow gameView, Type gameViewType, int originalSizeIndex)
        {
            PropertyInfo sizeIndexProp = gameViewType.GetProperty("selectedSizeIndex", BindingFlags.Instance | BindingFlags.Public);
            sizeIndexProp.SetValue(gameView, originalSizeIndex, null);
            EditorApplication.isPaused = false;
        }

        private class GameViewSize : GameViewSizeHelper.GameViewSize
        {
            public GameViewSize(int width, int height, string baseText)
            {
                type = GameViewSizeHelper.GameViewSizeType.FixedResolution;
                this.width = width;
                this.height = height;
                this.baseText = baseText;
            }
        }
    }
}