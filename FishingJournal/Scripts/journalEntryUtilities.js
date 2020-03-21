import Vue from 'vue/dist/vue';
import axios from 'axios/dist/axios';

Vue.component('journal-entry', {
    props: {
        latValue: String,
        longValue: String,
        locationOverride: String,
        weatherSummaryValue: String,
        dateValue: String,
        notesValue: String
    },
    computed: {
        latAndLong() {
            return this.fields.Latitude.value ? `Lat: ${this.fields.Latitude.value}, Long: ${this.fields.Longitude.value}` : null;      
        }
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
            retrievingLocation: false,
            retrievingWeather: false
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
            this.retrievingLocation = true;
            navigator.geolocation.getCurrentPosition((x) => {
                let lat = x.coords.latitude;
                let long = x.coords.longitude;
                this.fields.Latitude.value = lat;
                this.fields.Longitude.value = long;
                this.retrievingLocation = false;
            });
        },
        getWeather() {
            this.retrievingWeather = true;
            let nearestStationId = '';
            axios.get(`https://api.meteostat.net/v1/stations/nearby?lat=${this.fields.Latitude.value}&lon=${this.fields.Longitude.value}&limit=1&key=1SOl29tZ`)
                .then(response => {
                    nearestStationId = response.data.data[0].id;
                    let today = new Date().toISOString().split('T')[0];
                    axios.get(`https://api.meteostat.net/v1/history/hourly?station=${nearestStationId}&start=${today}&end=${today}&key=1SOl29tZ`).then(response => {
                        const length = response.data.data.length-1;
                        this.fields.WeatherSummary.value = `Temperature: ${(response.data.data[length].temperature)* 9 / 5 + 32}F \n` +
                        `Humidity: ${(response.data.data[length].humidity)}% \n` +
                        `Precipitation: ${(response.data.data[length].precipitation) / 25.4} inches \n` +
                        `Barometric Pressure: ${(response.data.data[length].pressure)} hPa \n` +
                        `Wind Speed: ${(response.data.data[length].windspeed)/1.609} mph \n` +
                        `Wind Direction: ${(response.data.data[length].winddirection)} degrees \n`;
                        
                        this.retrievingWeather = false;
                    });
                });
        }
    }
});

new Vue({
    el: '#root'
});