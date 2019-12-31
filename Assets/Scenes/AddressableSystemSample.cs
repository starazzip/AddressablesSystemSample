using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using System.IO;
using System;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class AddressableSystemSample : MonoBehaviour
{
    [SerializeField] private Slider _progressBar = null;
    [SerializeField] private Text _progressPercentText = null;
    private AsyncOperationHandle _downloadAsyncOperation;
    private bool _startDownload = false;
    private string _cachePath = @"D:/AddressablesCache";
    private SceneInstance _currentScene;
    private int _loadingSceneId;
    void Update()
    {
        if (_startDownload)
        {
            SetProgress(_downloadAsyncOperation.PercentComplete);
        }
    }
    void Start()
    {
        if (!Directory.Exists(_cachePath))
            Directory.CreateDirectory(_cachePath);
        Caching.currentCacheForWriting = Caching.AddCache(_cachePath);
        Addressables.InitializeAsync().Completed += InitCompletedCallback;
    }

    private void InitCompletedCallback(AsyncOperationHandle<IResourceLocator> iniResult)
    {
        if (IsAsyncSucceded(iniResult))
        {
            Addressables.CheckForCatalogUpdates().Completed += CheckCataLogsCompleted;
        }
    }

    private void CheckCataLogsCompleted(AsyncOperationHandle<List<string>> checkCataLogsResult)
    {
        if (IsAsyncSucceded(checkCataLogsResult))
        {
            if (checkCataLogsResult.Result.Count > 0)
            {
                Addressables.UpdateCatalogs().Completed += UpdateCataLogsCompleted;
            }
            else
            {
                StartDownload();
            }
        }
    }

    private void UpdateCataLogsCompleted(AsyncOperationHandle<List<IResourceLocator>> updateCataLogsResult)
    {
        if (IsAsyncSucceded(updateCataLogsResult))
        {
            StartDownload();
        }
    }

    private void StartDownload()
    {
        _startDownload = true;
        _downloadAsyncOperation = Addressables.DownloadDependenciesAsync("AllAddressables");
        _downloadAsyncOperation.Completed += DownloadCompleted;
    }

    private void DownloadCompleted(AsyncOperationHandle downloadResult)
    {
        if (downloadResult.Status == AsyncOperationStatus.Succeeded)
        {
            _startDownload = false;
            SetProgress(1.0f);
        }
        else
        {
            UnityEngine.Debug.LogError("Error: " + downloadResult.OperationException.Message);
        }
    }

    private void SetProgress(float percentComplete)
    {
        _progressBar.value = percentComplete;
        string percent = percentComplete * 100 + "%";
        _progressPercentText.text = percent;
        UnityEngine.Debug.Log(percent);
    }

    private bool IsAsyncSucceded<T>(AsyncOperationHandle<T> iniResult)
    {
        if (iniResult.Status == AsyncOperationStatus.Succeeded)
        {
            return true;
        }
        else
        {
            UnityEngine.Debug.LogError("Error: " + iniResult.OperationException.Message);
            return false;
        }
    }
    public void onClickBtn(int sceneId)
    {
        if (sceneId == _loadingSceneId) return;
        _loadingSceneId = sceneId;
        if (_currentScene.Scene.isLoaded)
        {
            Addressables.UnloadSceneAsync(_currentScene).Completed += UnloadSceneCompleted;
        }
        else
        {
            LoadScene();
        }
    }

    private void UnloadSceneCompleted(AsyncOperationHandle<SceneInstance> unloadReuslt)
    {
        if (IsAsyncSucceded(unloadReuslt))
        {
            LoadScene();
        }
    }

    private void LoadScene()
    {
        Addressables.LoadSceneAsync("Assets/Scenes/Scene" + _loadingSceneId + ".unity", LoadSceneMode.Additive).Completed += LoadSceneCompleted;
    }

    private void LoadSceneCompleted(AsyncOperationHandle<SceneInstance> LoadResult)
    {
        if (IsAsyncSucceded(LoadResult))
        {
            _currentScene = LoadResult.Result;
        }
    }
}
