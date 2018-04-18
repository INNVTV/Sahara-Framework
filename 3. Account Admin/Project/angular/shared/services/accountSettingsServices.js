(function () {
    'use strict';

    var serviceId = 'accountSettingsServices';
    var urlBase = '/AccountSettings/Json';

    // Inject into main app module:
    angular.module('app')
        .factory(serviceId, ['$http', accountSettingsServices]);

    function accountSettingsServices($http) {
        // Define the functions and properties to reveal.
        var service = {

            //Get -----------------------------
            getAccountSettings: getAccountSettings,
            getThemes: getThemes,

            //Update -----------------------------
            updateAccountTheme: updateAccountTheme,

            updateContactInfo: updateContactInfo,
            
            updateShowPhoneNumber: updateShowPhoneNumber,
            updateShowEmail: updateShowEmail,
            updateShowAddress: updateShowAddress,

            updateUseSalesLeads: updateUseSalesLeads,
            updateUseSalesAlerts: updateUseSalesAlerts,

            addSalesAlertEmail: addSalesAlertEmail,
            removeSalesAlertEmail: removeSalesAlertEmail,

            updateCustomDomain: updateCustomDomain,

            updateSalesLeadsButtonCopy: updateSalesLeadsButtonCopy,
            updateSalesLeadsDescriptionCopy: updateSalesLeadsDescriptionCopy
        };

        return service;

        /* ===============================================================

         ACCOUNT

       ================================================================*/

        /* ===========  GET  ==========================================*/

        function getAccountSettings() {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/GetAccountSettings' + cacheBuster });
        }

        function getThemes() {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/GetThemes' + cacheBuster });
        }

        /* ===========  UPDATE  ==========================================*/

        function updateAccountTheme(themeName) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdateTheme' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    themeName: themeName
                }
            });
        }

        function updateContactInfo(phoneNumber, email, address1, address2, city, state, postalCode) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdateContactInfo' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    phoneNumber: phoneNumber,
                    email: email,
                    address1: address1,
                    address2: address2,
                    city: city,
                    state: state,
                    postalCode: postalCode                   
                }
            });
        }

        function updateShowPhoneNumber(showPhoneNumber) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdateShowPhoneNumber' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    showPhoneNumber: showPhoneNumber
                }
            });
        }

        function updateShowEmail(showEmail) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdateShowEmail' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    showEmail: showEmail
                }
            });
        }

        function updateShowAddress(showAddress) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdateShowAddress' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    showAddress: showAddress
                }
            });
        }

        function updateUseSalesLeads(useSalesLeads) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdateUseSalesLeads' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    useSalesLeads: useSalesLeads
                }
            });
        }

        function updateUseSalesAlerts(useSalesAlerts) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdateUseSalesAlerts' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    useSalesAlerts: useSalesAlerts
                }
            });
        }

        function addSalesAlertEmail(email) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/AddSalesAlertEmail' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    email: email
                }
            });
        }
        

        function removeSalesAlertEmail(index) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/RemoveSalesAlertEmail' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    index: index
                }
            });
        }

        function updateSalesLeadsButtonCopy(buttonCopy) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdateSalesLeadsButtonCopy' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    buttonCopy: buttonCopy
                }
            });
        }

        function updateSalesLeadsDescriptionCopy(descriptionCopy) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdateSalesLeadsDescriptionCopy' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    descriptionCopy: descriptionCopy
                }
            });
        }

        function updateCustomDomain(customDomain) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdateCustomDomain' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    customDomain: customDomain
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