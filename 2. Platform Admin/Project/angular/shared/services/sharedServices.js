(function () {
    'use strict';

    var serviceId = 'sharedServices';
    var profileUrlBase = '/Profile/Json';
    var notificationUrlBase = '/Communication/Notifications/Json';

    // Inject into main app module:
    angular.module('app')
        .factory(serviceId, ['$http', sharedServices]);

    function sharedServices($http) {
        // Define the functions and properties to reveal.
        var service = {

            //profile -----------------------------
            getCurrentUserProfile: getCurrentUserProfile,

            //Get Notifications -------------------
            getNotifications: getNotifications,
            getNotificationsByType: getNotificationsByType,

            //Send Notifications -------------------
            sendNotificationToBulkAccounts: sendNotificationToBulkAccounts,
            sendNotificationToAccount: sendNotificationToAccount,
            sendNotificationToUser: sendNotificationToUser,



        };

        return service;


        /* ==========================================
          PROFILE
        ==========================================*/

        function getCurrentUserProfile() {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({ method: 'GET', url: profileUrlBase + '/GetCurrentUser/' + cacheBuster });
        }


        /* ==========================================
          NOTIFICATIONS
        ==========================================*/

        function getNotifications(notificationStatus, userId) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({ method: 'GET', url: notificationUrlBase + '/GetUserNotifications/' + notificationStatus + '/' + userId + cacheBuster });
        }

        function getNotificationsByType(notificationType, notificationStatus, userId) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({ method: 'GET', url: notificationUrlBase + '/GetUserNotificationsByType/' + notificationType + '/' + notificationStatus + '/' + userId + cacheBuster });
        }


        function sendNotificationToBulkAccounts(notificationType, notificationMessage, expirationMinutes, accountOwnersOnly, columnName, columnValue) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: notificationUrlBase + '/SendNotificationToBulkAccounts/' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    notificationType: notificationType,
                    notificationMessage: notificationMessage,
                    expirationMinutes: expirationMinutes,
                    accountOwnersOnly: accountOwnersOnly,
                    columnName: columnName,
                    columnValue: columnValue
                }
            });
        }


        function sendNotificationToAccount(notificationType, accountId, notificationMessage, expirationMinutes, accountOwnersOnly) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: notificationUrlBase + '/SendNotificationToAccount' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    notificationType: notificationType,
                    accountId: accountId,
                    notificationMessage: notificationMessage,
                    expirationMinutes: expirationMinutes,
                    accountOwnersOnly: accountOwnersOnly,
                }
            });
        }

        function sendNotificationToUser(notificationType, userId, notificationMessage, expirationMinutes) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: notificationUrlBase + '/SendNotificationToUser' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    notificationType: notificationType,
                    userId: userId,
                    notificationMessage: notificationMessage,
                    expirationMinutes: expirationMinutes,
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