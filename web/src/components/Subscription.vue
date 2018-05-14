<template>
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
</template>

<script>
    import Account from './Account'

    export default {
        name: 'Subscription',
        components: {
            Account
        },
        props: ['id', 'name'],
        data () {
            return {
                accounts: []
            }
        },
        mounted () {
            var vm = this;

            axios.get('/api/accounts/' + this.id)
                .then(function (response) {
                    vm.accounts = response.data.accounts;
                })
                .catch(function (error) {
                    console.log(error);
                });
        }
    }
</script>

<style>
    .account-list {
        padding: 0;
    }
</style>