(function () {
    'use strict';

    var serviceId = 'plansServices';
    var urlBase = '/Plans/Json';

    // Inject into main app module:
    angular.module('app')
        .factory(serviceId, ['$http', plansServices]);

    function plansServices($http) {
        // Define the functions and properties to reveal.
        var service = {

            // Get methods:
            getPlans: getPlans,
            getPlan: getPlan,
            getFrequencies: getFrequencies,

            // Create methods
            createPlan: createPlan,

            // Update methods
            updatePlanVisibility: updatePlanVisibility,
            //updatePlanMaxUsers: updatePlanMaxUsers,
            //updatePlanMaxCategories: updatePlanMaxCategories,
            //updatePlanMaxSubcategories: updatePlanMaxSubcategories,
            //updatePlanMaxTags: updatePlanMaxTags,
            //updatePlanMaxImages: updatePlanMaxImages,
            //updatePlanAllowImageEnhancements: updatePlanAllowImageEnhancements,

            // Subscribe/Update account methods:
            subscribeAccount: subscribeAccount,
            upgradePathAvailable: upgradePathAvailable,
            updateAccountPlan: updateAccountPlan,

            // Delete methods
            deletePlan: deletePlan,
        };

        return service;

        /* ==========================================
               GET Methods
        ==========================================*/

        function getPlans() {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/GetPlans' + cacheBuster });
        }

        function getPlan(planName) {

            var date = new Date();
            var cacheBuster = "&cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/GetPlan?planName=' + planName + cacheBuster });
        }

        function getFrequencies() {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/GetFrequencies' + cacheBuster });
        }

        /* ==========================================
               POST Methods
        ========================================== */
        // ---------- CREATE ---------

        function createPlan(paymentPlanName, monthlyRate, maxUsers, maxCategorizationsPerSet, maxProductsPerSet, maxProperties, maxValuesPerProperty, maxTags,
            allowSalesLeads, allowLocationData, allowCustomOrdering, allowThemes, allowImageEnhancements, basicSupport, enhancedSupport,
            maxImageGroups, maxImageFormats, maxImageGalleries, maxImagesPerGallery, visible) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/CreatePlan' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    paymentPlanName: paymentPlanName,
                    monthlyRate: monthlyRate,
                    maxUsers: maxUsers,
                    maxCategorizationsPerSet: maxCategorizationsPerSet,
                    maxProductsPerSet: maxProductsPerSet,
                    maxProperties: maxProperties,
                    maxValuesPerProperty: maxValuesPerProperty,
                    maxTags: maxTags,
                    allowSalesLeads: allowSalesLeads,
                    allowLocationData: allowLocationData,
                    allowCustomOrdering: allowCustomOrdering,
                    allowThemes: allowThemes,
                    allowImageEnhancements: allowImageEnhancements,
                    basicSupport: basicSupport,
                    enhancedSupport: enhancedSupport,

                    maxImageGroups: maxImageGroups,
                    maxImageFormats: maxImageFormats,
                    maxImageGalleries: maxImageGalleries,
                    maxImagesPerGallery: maxImagesPerGallery,

                    visible: visible
                }
            });
        }

        // ---------- UPDATE ---------

        function updatePlanVisibility(paymentPlanName, isVisible) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdatePlanVisibility' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    paymentPlanName: paymentPlanName,
                    isVisible: isVisible
                }
            });
        }

        /*
        function updatePlanName(paymentPlanName, newName) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdatePlanName' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    paymentPlanName: paymentPlanName,
                    newName: newName
                }
            });
        }

        

        function updatePlanMaxUsers(paymentPlanName, newLimit) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdatePlanMaxUsers' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    paymentPlanName: paymentPlanName,
                    newLimit: newLimit
                }
            });
        }

        function updatePlanMaxCategories(paymentPlanName, newLimit) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdatePlanMaxCategories' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    paymentPlanName: paymentPlanName,
                    newLimit: newLimit
                }
            });
        }

        function updatePlanMaxSubcategories(paymentPlanName, newLimit) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdatePlanMaxSubcategories' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    paymentPlanName: paymentPlanName,
                    newLimit: newLimit
                }
            });
        }

        function updatePlanMaxTags(paymentPlanName, newLimit) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdatePlanMaxTags' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    paymentPlanName: paymentPlanName,
                    newLimit: newLimit
                }
            });
        }

        function updatePlanMaxImages(paymentPlanName, newLimit) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdatePlanMaxImages' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    paymentPlanName: paymentPlanName,
                    newLimit: newLimit
                }
            });
        }

        function updatePlanAllowImageEnhancements(paymentPlanName, allowImageEnhancements) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdatePlanAllowImageEnhancements' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    paymentPlanName: paymentPlanName,
                    allowImageEnhancements: allowImageEnhancements
                }
            });
        }
*/
        // ---------- DELETE ---------

        function deletePlan(paymentPlanName) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/DeletePlan' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    paymentPlanName: paymentPlanName,
                }
            });
        }

        /* ==========================================
               SUBSCRIPTION Methods
        ==========================================*/



        function subscribeAccount(accountId, planName, frequencyMonths, cardName, cardNumber, cvc, expirationMonth, expirationYear) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/SubscribeAccount' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    accountId: accountId,
                    planName: planName,
                    frequencyMonths: frequencyMonths,
                    cardName: cardName,
                    cardNumber: cardNumber,
                    cvc: cvc,
                    expirationMonth: expirationMonth,
                    expirationYear: expirationYear,
                }
            });
        }

        function upgradePathAvailable(planName) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpgradePathAvailable' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    planName: planName
                }
            });
        }

        function updateAccountPlan(accountId, planName, frequencyMonths) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdateAccountPlan' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    accountId: accountId,
                    planName: planName,
                    frequencyMonths: frequencyMonths,
                }
            });
        }

        /* ==========================================
                HELPERS
        ==========================================*/

        var transformRequest = function (data, headersGetter) {
            var headers = headersGetter();
            headers['Authorization'] = 'WSSE profile="UsernameToken"';
            headers['X-WSSE'] = 'UsernameToken ' + nonce
            headers['Content-Type'] = 'application/json';
        };

    }
})();