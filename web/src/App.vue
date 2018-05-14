<template>
    <div id="app">
        <ul id="subscription-list" class="collapsible" data-collapsible="accordion">
            <subscription
                v-for="subscription in subscriptions"
                v-bind:key="subscription.id"
                v-bind:name="subscription.name"
                v-bind:id="subscription.id"
            ></subscription>
        </ul>
    </div>
</template>

<script>

import Subscription from './components/Subscription'

export default {
  name: 'app',
  components: {
    Subscription
  },
  data () {
    return {
      subscriptions: []
    }
  },
  mounted () {
    var vm = this;

    axios.get('/api/subscriptions/')
        .then(function (response){
            vm.subscriptions = response.data.subscriptions;
        })
        .catch(function (error) {
            console.log(error);
        });
  }
}
</script>