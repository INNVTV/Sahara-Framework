(function () {
    'use strict';

    var serviceId = 'transferServices';
    var urlBase = '/Transfers/Json';

    // Inject into main app module:
    angular.module('app')
        .factory(serviceId, ['$http', transferServices]);

    function transferServices($http) {
        // Define the functions and properties to reveal.
        var service = {

            getTransfer: getTransfer,

            getTransferHistory: getTransferHistory,
            getTransferHistory_Next: getTransferHistory_Next,
            getTransferHistory_Last: getTransferHistory_Last,

            getTransferHistory_ByDateRange: getTransferHistory_ByDateRange,
            getTransferHistory_ByDateRange_Next: getTransferHistory_ByDateRange_Next,
            getTransferHistory_ByDateRange_Last: getTransferHistory_ByDateRange_Last,

        };

        return service;

        function getTransfer(transferId) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET', url: urlBase + '/GetTransfer' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    transferId: transferId
                }
            });
        }

        // ---------------------------------------------

        function getTransferHistory(itemLimit) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET', url: urlBase + '/GetTransferHistory' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    itemLimit: itemLimit
                }
            });
        }

        function getTransferHistory_Next(itemLimit, startingAfter) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET', url: urlBase + '/GetTransferHistory_Next' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    itemLimit: itemLimit,
                    startingAfter: startingAfter
                }
            });
        }

        function getTransferHistory_Last(itemLimit, endingBefore) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET', url: urlBase + '/GetTransferHistory_Last' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    itemLimit: itemLimit,
                    endingBefore: endingBefore
                }
            });
        }

        // ---------------------------------------------

        function getTransferHistory_ByDateRange(itemLimit, startDate, endDate) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET', url: urlBase + '/GetTransferHistory_ByDateRange' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    itemLimit: itemLimit,
                    startDate: startDate,
                    endDate: endDate
                }
            });
        }

        function getTransferHistory_ByDateRange_Next(itemLimit, startDate, endDate, startingAfter) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET', url: urlBase + '/GetTransferHistory_ByDateRange_Next' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    itemLimit: itemLimit,
                    startDate: startDate,
                    endDate: endDate,
                    startingAfter: startingAfter
                }
            });
        }

        function getTransferHistory_ByDateRange_Last(itemLimit, startDate, endDate, endingBefore) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET', url: urlBase + '/GetTransferHistory_ByDateRange_Last'  + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    itemLimit: itemLimit,
                    startDate: startDate,
                    endDate: endDate,
                    endingBefore: endingBefore
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