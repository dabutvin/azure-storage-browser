
Vue.component('subscription', {
    props: [ 'name' ],
    template: `
            <li>{{ name }}</li>
    `
});

var vm = new Vue({
    el: '#app',
    data: {
        subscriptions: []
    },
    mounted: function () {
        var vm = this;

        axios.get('/api/subscriptions/')
            .then(function (response){
                vm.subscriptions = response.data.subscriptions;
            })
            .catch(function (error) {
                console.log(error);
            });

        var x  = "";
    },
    methods: {

    }
});
