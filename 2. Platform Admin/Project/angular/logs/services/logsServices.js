(function () {
    'use strict';

    var serviceId = 'logsServices';
    var urlBase = '/Platform/Json';

    // Inject into main app module:
    angular.module('app')
        .factory(serviceId, ['$http', logsServices]);

    function logsServices($http) {
        // Define the functions and properties to reveal.
        var service = {

            //Logs -----------------------------
            getPlatformLog: getPlatformLog,
            getPlatformLogByCategory: getPlatformLogByCategory,
            getPlatformLogByActivity: getPlatformLogByActivity,
            getPlatformLogByAccount: getPlatformLogByAccount,
            getPlatformLogByUser: getPlatformLogByUser,
            //Log Types
            getPlatformLogCategories: getPlatformLogCategories,
            getPlatformLogActivities: getPlatformLogActivities,

        };

        return service;



        /* =========================================
           LOGS
        ===========================================*/

        function getPlatformLog(maxRecords) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET',
                url: urlBase + '/GetPlatformLog' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    maxRecords: maxRecords,
                }
            });
        }

        function getPlatformLogByCategory(categoryType, maxRecords) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET',
                url: urlBase + '/GetPlatformLogByCategory' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    categoryType: categoryType,
                    maxRecords: maxRecords,
                }
            });
        }

        function getPlatformLogByActivity(activityType, maxRecords) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET',
                url: urlBase + '/GetPlatformLogByActivity' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    activityType: activityType,
                    maxRecords: maxRecords,
                }
            });
        }

        function getPlatformLogByAccount(accountId, maxRecords) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET',
                url: urlBase + '/GetPlatformLogByAccount' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    accountId: accountId,
                    maxRecords: maxRecords,
                }
            });
        }

        function getPlatformLogByUser(userId, maxRecords) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET',
                url: urlBase + '/GetPlatformLogByUser' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    userId: userId,
                    maxRecords: maxRecords,
                }
            });
        }



        /* ==========================================
          LOG TYPES
        ==========================================*/

        function getPlatformLogCategories() {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/GetPlatformLogCategories' + cacheBuster });
        }

        function getPlatformLogActivities() {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/GetPlatformLogActivities' + cacheBuster });
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