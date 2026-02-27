// using System;
// using AES.Tools.VContainer.Installer;
// using Systems.IAP;
// using UnityEngine;
// using VContainer;
//
// namespace Game.IAP
// {
//     [CreateAssetMenu(menuName = "Game/IAP/IAP Integration Installer", fileName = "IapIntegrationInstaller")]
//     public sealed class IapIntegrationInstallerSO : ScriptableInstaller
//     {
//         [Header("Resources paths (no extension)")]
//         [SerializeField] private string iapProductPath = "IAP/Generated/IapProduct";
//         [SerializeField] private string iapStoreProductPath = "IAP/Generated/IapStoreProduct";
//         [SerializeField] private string iapBundleContentPath = "IAP/Generated/IapBundleContent";
//
//         public override void Install(IContainerBuilder builder)
//         {
//             // Load TextAssets (Resources)
//             var prodText = Resources.Load<TextAsset>(iapProductPath);
//             var storeText = Resources.Load<TextAsset>(iapStoreProductPath);
//             var bundleText = Resources.Load<TextAsset>(iapBundleContentPath);
//
//             if (prodText == null) throw new Exception($"Missing TextAsset: {iapProductPath}");
//             if (storeText == null) throw new Exception($"Missing TextAsset: {iapStoreProductPath}");
//             if (bundleText == null) throw new Exception($"Missing TextAsset: {iapBundleContentPath}");
//
//             var prods = JsonUtility.FromJson<JsonRoot<IapProductRow>>(prodText.text)?.rows ?? new();
//             var stores = JsonUtility.FromJson<JsonRoot<IapStoreProductRow>>(storeText.text)?.rows ?? new();
//             var bundles = JsonUtility.FromJson<JsonRoot<IapBundleContentRow>>(bundleText.text)?.rows ?? new();
//
//             // DB
//             var db = new IapDatabase(prods, stores, bundles);
//             builder.RegisterInstance(db);
//
//             // Replace catalog + reward handler for UnityIapService
//             builder.RegisterInstance(new Systems.IAP.JsonIapCatalog(prods, stores))
//                 .As<IIapCatalog>();
//
//             builder.Register<AES.IAP.Core.IapPurchaseResolver>(Lifetime.Singleton);
//
//             builder.Register<Systems.IAP.RewardHandler_FromDb>(Lifetime.Singleton)
//                 .As<Systems.IAP.IIapRewardHandler>();
//
//             // Game-specific apply + backend
//             builder.Register<UnityIapPurchaseBackend>(Lifetime.Singleton)
//                 .As<IIapPurchaseBackend>();
//
//             builder.Register<MyGameRewardApplier>(Lifetime.Singleton)
//                 .As<IIapRewardApplier>();
//
//             // TODO: 게임에 이 두 가지 구현을 제공하고 등록하세요.
//             // builder.Register<YourRemoveAdsService>(Lifetime.Singleton).As<IRemoveAdsService>();
//             // builder.Register<YourUnlimitedLivesService>(Lifetime.Singleton).As<IUnlimitedLivesService>();
//         }
//     }
// }
