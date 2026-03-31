using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
#if UNITY_PURCHASING
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class Purchaser : MonoBehaviour, IStoreListener
{
    public static Purchaser Instance;
    public delegate void IAPResult(int id);
    public static event IAPResult iAPResult;

    private static IStoreController m_StoreController;
    private static IExtensionProvider m_StoreExtensionProvider;

    public static string kProductIDConsumable = "coin_pack1";
    public static string kProductIDNonConsumable = "nonconsumable";
    public static string kProductIDSubscription = "subscription";

    public string item1 = "coin_pack1";
    public string item2 = "coin_pack2";
    public string item3 = "coin_pack3";
    public string removeAds = "remove_ads";

    private static string kProductNameAppleSubscription = "com.unity3d.subscription.new";
    private static string kProductNameGooglePlaySubscription = "com.unity3d.subscription.original";

    private IProgressService progressService;

    [Inject]
    private void Construct(IProgressService progressService)
    {
        this.progressService = progressService;
    }

    void Start()
    {
        ProjectScope.Inject(this);
        Instance = this;
        DontDestroyOnLoad(gameObject);
        if (m_StoreController == null)
            InitializePurchasing();
    }

    public void InitializePurchasing()
    {
        if (IsInitialized())
            return;

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        builder.AddProduct(kProductIDConsumable, ProductType.Consumable);
        builder.AddProduct(item1, ProductType.Consumable);
        builder.AddProduct(item2, ProductType.Consumable);
        builder.AddProduct(item3, ProductType.Consumable);
        builder.AddProduct(kProductIDNonConsumable, ProductType.NonConsumable);
        builder.AddProduct(removeAds, ProductType.NonConsumable);
        builder.AddProduct(kProductIDSubscription, ProductType.Subscription, new IDs()
        {
            { kProductNameAppleSubscription, AppleAppStore.Name },
            { kProductNameGooglePlaySubscription, GooglePlay.Name },
        });

        UnityPurchasing.Initialize(this, builder);
    }

    private bool IsInitialized()
    {
        return m_StoreController != null && m_StoreExtensionProvider != null;
    }

    public void BuyItem1()
    {
        BuyProductID(item1);
    }

    public void BuyItem2()
    {
        BuyProductID(item2);
    }

    public void BuyItem3()
    {
        BuyProductID(item3);
    }

    public void BuyConsumable()
    {
        BuyProductID(kProductIDConsumable);
    }

    public void BuyNonConsumable()
    {
        BuyProductID(kProductIDNonConsumable);
    }

    public void BuyRemoveAds()
    {
        BuyProductID(removeAds);
    }

    public void BuySubscription()
    {
        BuyProductID(kProductIDSubscription);
    }

    void BuyProductID(string productId)
    {
        if (IsInitialized())
        {
            Product product = m_StoreController.products.WithID(productId);
            if (product != null && product.availableToPurchase)
            {
                Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
                m_StoreController.InitiatePurchase(product);
            }
            else
            {
                Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
            }
        }
        else
        {
            Debug.Log("BuyProductID FAIL. Not initialized.");
        }
    }

    public void RestorePurchases()
    {
        if (!IsInitialized())
        {
            Debug.Log("RestorePurchases FAIL. Not initialized.");
            return;
        }

        if (Application.platform == RuntimePlatform.IPhonePlayer ||
            Application.platform == RuntimePlatform.OSXPlayer)
        {
            Debug.Log("RestorePurchases started ...");
            var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
            apple.RestoreTransactions((result) =>
            {
                Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
            });
        }
        else
        {
            Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
        }
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("OnInitialized: PASS");
        m_StoreController = controller;
        m_StoreExtensionProvider = extensions;
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        if (String.Equals(args.purchasedProduct.definition.id, kProductIDConsumable, StringComparison.Ordinal))
        {
            Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
            iAPResult(1);
        }
        else if (String.Equals(args.purchasedProduct.definition.id, item1, StringComparison.Ordinal))
        {
            Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
            iAPResult(1);
        }
        else if (String.Equals(args.purchasedProduct.definition.id, item2, StringComparison.Ordinal))
        {
            Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
            iAPResult(2);
        }
        else if (String.Equals(args.purchasedProduct.definition.id, item3, StringComparison.Ordinal))
        {
            Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
            iAPResult(3);
        }
        else if (String.Equals(args.purchasedProduct.definition.id, removeAds, StringComparison.Ordinal))
        {
            (progressService ?? ProjectScope.Resolve<IProgressService>()).RemoveAds = true;

            Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
            Debug.Log("Ads removed");
        }
        else if (String.Equals(args.purchasedProduct.definition.id, kProductIDNonConsumable, StringComparison.Ordinal))
        {
            Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
        }
        else if (String.Equals(args.purchasedProduct.definition.id, kProductIDSubscription, StringComparison.Ordinal))
        {
            Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
        }
        else
        {
            Debug.Log(string.Format("ProcessPurchase: FAIL. Unrecognized product: '{0}'", args.purchasedProduct.definition.id));
        }

        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
    }
}
#else
public class Purchaser : MonoBehaviour
{
    public void BuyItem1()
    {
        Debug.LogError("You need to turn on IAP in Windown/Services tab to use this feature");
    }

    public void BuyItem2()
    {
        Debug.LogError("You need to turn on IAP in Windown/Services tab to use this feature");
    }

    public void BuyRemoveAds()
    {
        Debug.LogError("You need to turn on IAP in Windown/Services tab to use this feature");
    }
}
#endif
