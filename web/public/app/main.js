Vue.component('account', {
    props: ['name', 'id'],
    data: function () {
        return {
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
    methods: {
        table: function (event) {
            alert('table ' + this.key)
        },
        queue: function (event) {
            alert('queue ' + this.key)
        },
        blob: function (event) {
            alert('blob ' + this.key)
        }
    },
    template: `
            <li class="collection-item">
                <div>
                    {{ name }}
                    <button v-on:click="table" class="secondary-content">
                        <i class="material-icons">table_chart</i>
                    </button>
                    <button v-on:click="queue" class="secondary-content">
                        <i class="material-icons">merge_type</i>
                    </button>
                    <button v-on:click="blob" class="secondary-content">
                        <i class="material-icons">folder_open</i>
                    </button>
                </div>
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
                <div class="collapsible-header">
                    {{ name }}
                </div>
                <ul class="collection collapsible-body account-list">
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
