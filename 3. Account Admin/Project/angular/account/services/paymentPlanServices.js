(function () {
    'use strict';

    var serviceId = 'paymentPlanServices';
    var urlBase = '/PaymentPlans/Json';

    // Inject into main app module:
    angular.module('app')
        .factory(serviceId, ['$http', paymentPlanServices]);

    function paymentPlanServices($http) {
        // Define the functions and properties to reveal.
        var service = {

            // Get methods:
            getPlans: getPlans,
            getPlan: getPlan,
            getFrequencies: getFrequencies,
            //getCardInfo: getCardInfo,

            // Subscribe methods:
            subscribe: subscribe,
            subscribeFromSub: subscribeFromSub, //<-- Used for unauthenticated sub controller, we have to add accountId

            // Update methods
            updatePlan: updatePlan,
            //updateCard: updateCard

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

        //function getCardInfo() {

            //var date = new Date();
            //var cacheBuster = "?cb=" + date.getTime();

            //return $http({ method: 'GET', url: urlBase + '/GetCardInfo' + cacheBuster });
        //}


        /* ==========================================
               POST Methods
        ==========================================*/



        function subscribe(planName, frequencyMonths, cardName, cardNumber, cvc, expirationMonth, expirationYear) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/Subscribe' + cacheBuster,
                transformRequest: transformRequest,
                params: {
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

        function subscribeFromSub(accountId, planName, frequencyMonths, cardName, cardNumber, cvc, expirationMonth, expirationYear) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/SubscribeFromSub' + cacheBuster,
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

        function updatePlan(planName, frequencyMonths) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdatePlan' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    planName: planName,
                    frequencyMonths: frequencyMonths,
                }
            });
        }

        /*
        function updateCard(cardName, cardNumber, cvc, expirationMonth, expirationYear) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdateCard' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    cardName: cardName,
                    cardNumber: cardNumber,
                    cvc: cvc,
                    expirationMonth: expirationMonth,
                    expirationYear: expirationYear,
                }
            });
        }*/



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