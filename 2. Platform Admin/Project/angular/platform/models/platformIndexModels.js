(function () {
    'use strict';

    var serviceId = 'platformIndexModels';

    // Inject into main app module:
    angular.module('app')
        .factory(serviceId, [platformIndexModels]);

    function platformIndexModels() {
        // Define the functions and properties to reveal.
        var service = {

            //Lists
            getBulkAccountFilters: getBulkAccountFilters,

        };

        return service;

        /* ==========================================
               LISTS
           ==========================================*/


        function getBulkAccountFilters() {

            return [
                
                //{ label: 'Trial Accounts', name: 'Accounts.PaymentPlanName', value: 'Trial' },
                //{ label: 'Free Accounts', name: 'Accounts.PaymentPlanName', value: 'Free' },
                //{ label: 'Small Accounts', name: 'Accounts.PaymentPlanName', value: 'Small' },
                { label: 'All Accounts', name: '', value: '' }
            ];
        }





    }

})();