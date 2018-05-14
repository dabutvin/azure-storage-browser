<template>
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
</template>

<script>
export default {
    name: 'Account',
    props: ['name', 'id'],
    data () {
        return {
            key: ''
        }
    },
    mounted () {
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
            alert('table? ' + this.key)
        },
        queue: function (event) {
            alert('queue ' + this.key)
        },
        blob: function (event) {
            alert('blob; ' + this.key)
        }
    }
}
</script>