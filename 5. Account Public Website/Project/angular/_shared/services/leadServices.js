(function () {
    'use strict';

    var serviceId = 'leadServices';
    var urlBase = 'https://';

    // Inject into main app module:
    angular.module('app')
        .factory(serviceId, ['$http', leadServices]);

    function leadServices($http) {
        // Define the functions and properties to reveal.
        var service = {

            // Get methods:
            submitLead: submitLead,

        };

        return service;

        /* ==========================================
               GET Methods
        ==========================================*/

        function submitLead(accountNameKey, apiDomain, firstName, lastName, companyName, phone, email, comments, productName, productId, fullyQualifiedName, locationPath, origin, ipAddress, userId, userName)
        {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({

                method: 'POST',

                url: urlBase + accountNameKey + '.' + apiDomain + '/lead/' + cacheBuster,

                transformRequest: transformRequest,
                params: {
                    firstName: firstName,
                    lastName: lastName,
                    companyName: companyName,
                    phone: phone,
                    email: email,
                    comments: comments,
                    productName: productName,
                    productId: productId,
                    fullyQualifiedName: fullyQualifiedName,
                    locationPath: locationPath,
                    origin: origin,
                    ipAddress: ipAddress,
                    userId: userId,
                    userName: userName,
                },


            });

        }


        /* ==========================================
                HELPERS
        ==========================================*/

        var transformRequest = function (data, headersGetter) {
            var headers = headersGetter();
            headers['Authorization'] = 'WSSE profile="UsernameToken"';
            headers['X-WSSE'] = 'UsernameToken ' + nonce;
            headers['Content-Type'] = 'application/json';
        };

    }
})();
