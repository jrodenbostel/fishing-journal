import Vue from 'vue/dist/vue';

Vue.component('vue-navbar', {
    data() {
        return {
            width: 0,
            height: 0,
            burgerActive: false
        }
    },
    methods: {
        toggle(e) {
            this.burgerActive = !this.burgerActive;
        },
        handleResize() {
            this.width = window.innerWidth;
            this.height = window.innerHeight;
            this.burgerActive = false;
        }
    },
    created() {
        window.addEventListener('resize', this.handleResize);
        this.handleResize();
    },
    destroyed() {
        window.removeEventListener('resize', this.handleResize);
    }
});

new Vue({
    el: '#navbar-root'
});