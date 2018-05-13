Vue.component('account', {
    props: ['name', 'id'],
    data: function () {
        return {
            name: '',
            key: ''
        }
    },
    mounted: function () {
        var vm = this;

        axios.get('/api/key/?id=' + this.id)
            .then(function (response) {
                vm.key = response.data.key;
            })
            .catch(function (error) {
                console.log(error);
            });
    },
    template: `
            <li>
                {{ name }}
            </li>
    `
});


Vue.component('subscription', {
    props: [ 'id', 'name' ],
    data: function () {
        return {
            accounts: []
        }
    },
    mounted: function () {
        var vm = this;

        axios.get('/api/accounts/' + this.id)
            .then(function (response) {
                vm.accounts = response.data.accounts;
            })
            .catch(function (error) {
                console.log(error);
            });

    },
    template: `
            <li>
                {{ name }}
                <ul>
                    <account
                        v-for="account in accounts"
                        v-bind:key="account.id"
                        v-bind:name="account.name"
                        v-bind:id="account.id"
                    ></account>
                </ul>
            </li>
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
    }
});
