var plugin = {
    _VKWebAppInit: function() {
        // Используем VK.WebAppInit, если доступен (в VK Mini Apps)
        if (typeof VK !== 'undefined' && typeof VK.WebAppInit === 'function') {
            VK.WebAppInit();
            console.log('[VK] VK.WebAppInit called');
            return;
        }
        // Запасной вариант
        if (typeof VKWebAppInit === 'function') {
            VKWebAppInit();
            console.log('[VK] VKWebAppInit called');
            return;
        }
        console.warn('[VK] VK Bridge not available');
    },

    _VKWebAppShowNativeAds: function(adFormat, waterfall) {
        if (typeof VKWebAppShowNativeAds === 'function') {
            VKWebAppShowNativeAds(Pointer_stringify(adFormat), waterfall);
        } else {
            console.warn('[VK] VKWebAppShowNativeAds not available');
        }
    },

    _VKWebAppShowWallPostBox: function(text) {
        if (typeof VKWebAppShowWallPostBox === 'function') {
            VKWebAppShowWallPostBox(Pointer_stringify(text));
        } else {
            console.warn('[VK] VKWebAppShowWallPostBox not available');
        }
    },

    _VKWebAppStorageSet: function(key, value) {
        if (typeof VKWebAppStorageSet === 'function') {
            VKWebAppStorageSet(Pointer_stringify(key), Pointer_stringify(value));
        } else {
            console.warn('[VK] VKWebAppStorageSet not available');
        }
    },

    _VKWebAppStorageGet: function(key) {
        if (typeof VKWebAppStorageGet === 'function') {
            VKWebAppStorageGet(Pointer_stringify(key));
        } else {
            console.warn('[VK] VKWebAppStorageGet not available');
        }
    },

    _VKWebAppShowLeaderBoardBox: function(value) {
        if (typeof VKWebAppShowLeaderBoardBox === 'function') {
            VKWebAppShowLeaderBoardBox(Pointer_stringify(value));
        } else {
            console.warn('[VK] VKWebAppShowLeaderBoardBox not available');
        }
    },

    _VKWebAppShowInviteBox: function() {
        if (typeof VKWebAppShowInviteBox === 'function') {
            VKWebAppShowInviteBox();
        } else {
            console.warn('[VK] VKWebAppShowInviteBox not available');
        }
    },

    _VKWebAppShowSubscriptionBox: function(action, item, subscription_id) {
        if (typeof VKWebAppShowSubscriptionBox === 'function') {
            VKWebAppShowSubscriptionBox(
                Pointer_stringify(action),
                Pointer_stringify(item),
                Pointer_stringify(subscription_id)
            );
        } else {
            console.warn('[VK] VKWebAppShowSubscriptionBox not available');
        }
    },

    _VKWebAppAccelerometerStart: function(refresh_rate) {
        if (typeof VKWebAppAccelerometerStart === 'function') {
            VKWebAppAccelerometerStart(refresh_rate);
        } else {
            console.warn('[VK] VKWebAppAccelerometerStart not available');
        }
    },

    _VKWebAppAccelerometerStop: function() {
        if (typeof VKWebAppAccelerometerStop === 'function') {
            VKWebAppAccelerometerStop();
        } else {
            console.warn('[VK] VKWebAppAccelerometerStop not available');
        }
    },

    getInfoUser: function(gameObject, method) {
        if (typeof getInfoUser === 'function') {
            getInfoUser(Pointer_stringify(gameObject), Pointer_stringify(method));
        } else {
            console.warn('[VK] getInfoUser not available');
        }
    },

    consoleLoge: function(value) {
        console.log(Pointer_stringify(value));
    },

    _Send: function(name, Params) {
        if (typeof _Send === 'function') {
            _Send(Pointer_stringify(name), Pointer_stringify(Params));
        } else {
            console.warn('[VK] _Send not available');
        }
    },

    // ============================================
    // КРИТИЧЕСКИ ВАЖНО - добавляем проверку готовности
    // ============================================
    IsBridgeReadyJS: function() {
        // Возвращаем 1 (true) если мост готов, иначе 0 (false)
        return window.__vkBridgeReady === true ? 1 : 0;
    }
};

mergeInto(LibraryManager.library, plugin);