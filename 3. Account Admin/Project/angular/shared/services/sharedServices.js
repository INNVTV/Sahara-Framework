(function () {
    'use strict';

    var serviceId = 'sharedServices';
    var profileUrlBase = '/Profile/Json';

    // Inject into main app module:
    angular.module('app')
        .factory(serviceId, ['$http', sharedServices]);

    function sharedServices($http) {
        // Define the functions and properties to reveal.
        var service = {

            //profile -----------------------------
            getCurrentUserProfile: getCurrentUserProfile,
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