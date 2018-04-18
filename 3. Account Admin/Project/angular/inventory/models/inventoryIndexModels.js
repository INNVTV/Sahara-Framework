(function () {
    'use strict';

    var serviceId = 'inventoryIndexModels';

    // Inject into main app module:
    angular.module('app')
        .factory(serviceId, [inventoryIndexModels]);

    function inventoryIndexModels() {
        // Define the functions and properties to reveal.
        var service = {

            //Lists
            getListView: getListView,

        };

        return service;

        /* ==========================================
               LISTS
           ==========================================*/

        function getListView() {

            return [
                { label: 'Choose Item',          name: '*',                 value: '*' },
                { label: 'Item One',             name: 'One',               value: '1' },
                { label: 'Item Two',             name: 'Two',               value: '2' },
                { label: 'Item Three',           name: 'Three',             value: '3' },
                { label: 'Item Four',            name: 'Four',              value: '4' },
            ];
        }




    }

})();