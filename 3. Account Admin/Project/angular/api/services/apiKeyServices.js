(function () {
    'use strict';

    var serviceId = 'apiKeyServices';
    var urlBase = '/Api/Json';

    // Inject into main app module:
    angular.module('app')
        .factory(serviceId, ['$http', apiKeyServices]);

    function apiKeyServices($http) {
        // Define the functions and properties to reveal.
        var service = {

            // Get methods ------------------
            getApiKeys: getApiKeys,

            // Generate methods ------------------
            generateApiKey: generateApiKey,

            // Regenerate methods ------------------
            regenerateApiKey: regenerateApiKey,

            // Delete methods ------------------
            deleteApiKey: deleteApiKey,
            
        };

        return service;

        /* ==========================================
               GET Methods
        ==========================================*/

        function getApiKeys() {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/GetApiKeys' + cacheBuster });
        }



        /* ==========================================
               POST Methods (Create)
        ==========================================*/

        function generateApiKey(name, description) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/GenerateApiKey' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    name: name,
                    description: description
                }
            });
        }


        /* ==========================================
               POST Methods (Update)
        ==========================================*/

        function regenerateApiKey(apiKey) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/RegenerateApiKey' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    apiKey: apiKey,
                }
            });
        }

        /* ==========================================
               POST Methods (Delete)
        ==========================================*/

        function deleteApiKey(apiKey) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/DeleteApiKey' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    apiKey: apiKey,
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