import Vue from 'vue/dist/vue';

Vue.component('journal-entry', {
    props: {
        latValue: String,
        longValue: String,
        locationOverride: String,
        weatherSummaryValue: String,
        dateValue: String,
        notesValue: String
    },
    data() {
        return {
            fields: {
                Latitude: { value: this.latValue, error: null },
                Longitude: { value: this.longValue, error: null },
                LocationOverride: { displayName: 'Location', value: this.locationOverride, error: null },
                WeatherSummary: { displayName: 'Weather Summary', value: this.weatherSummaryValue, error: null },
                Date: { value: this.dateValue, error: null },
                Notes: { value: this.notesValue, error: null },
            },
            retrieving: false
        }
    },
    methods:{
        checkForm(e) {
            let valid = true;
            for (const fieldName in this.fields) {
                if(Object.prototype.hasOwnProperty.call(this.fields, fieldName)) {
                    if(!this.fields[fieldName].value) {
                        this.fields[fieldName].error = `${this.fields[fieldName].displayName ? this.fields[fieldName].displayName : fieldName} is required.`;
                        valid = false;
                    }
                    else {
                        this.fields[fieldName].error = null;
                    }
                }
            }
            
            if(!valid) {
                e.preventDefault();
            }
            return true;
        },
        getLocation() {
            this.retrieving = true;
            navigator.geolocation.getCurrentPosition((x) => {
                let lat = x.coords.latitude;
                let long = x.coords.longitude;
                this.fields.Latitude.value = lat;
                this.fields.Longitude.value = long;
                this.retrieving = false;
            });
        }
    }
});

new Vue({
    el: '#root'
});