(function () {
    'use strict';

    var serviceId = 'accountDetailModels';

    // Inject into main app module:
    angular.module('app')
        .factory(serviceId, [accountDetailModels]);

    function accountDetailModels() {
        // Define the functions and properties to reveal.
        var service = {

            // Trial Models ------
            getTrialExtenstionDays: getTrialExtenstionDays,

            // Notification Models -----
            getNotificationTypes: getNotificationTypes,
            getNotificationRecipientTypes: getNotificationRecipientTypes
        };

        return service;

        function getTrialExtenstionDays() {

            return [
                { label: 'Extend trial', value: null },
                /*Values used for platform testing ======
                { label: 'Remove 5 Days', value: '-5' },
                { label: 'Remove 1 Day', value: '-1' },
                { label: 'Remove 1/2 Day', value: '-.5' },
                { label: 'Remove 1/4 Day', value: '-.25' },
                { label: 'Add 1/4 Day',              value: '.25' },
                { label: 'Add 1/2 Day', value: '.5' },*/
                { label: 'Add 1 Day', value: '1' },
                { label: 'Add 5 Days', value: '5' },
                { label: 'Add 10 Days', value: '10' },
                { label: 'Add 30 Days', value: '30' },
                /*{ label: 'Add 45 Days', value: '45' },
                { label: 'Add 90 Days', value: '90' }*/
            ];
        }

        function getNotificationTypes() {

            return [

                { label: 'Information', name: 'Information', value: 'Information', faClass: 'fa-info-circle', cssClass: 'btn-primary' },
                { label: 'Success', name: 'Success', value: 'Success', faClass: 'fa-check', cssClass: 'btn-success' },
                { label: 'Warning', name: 'Warning', value: 'Warning', faClass: 'fa-warning', cssClass: 'btn-warning' },
                { label: 'Alert', name: 'Alert', value: 'Alert', faClass: 'fa-warning', cssClass: 'btn-danger' },
            ];
        }

        function getNotificationRecipientTypes() {

            return [

                { label: 'Only Account Owners', name: 'Owners', value: 'owners' },
                { label: 'All Account Users', name: 'All', value: 'all' },
                { label: 'A Specific User', name: 'One', value: 'one' },
            ];
        }

    }

})();