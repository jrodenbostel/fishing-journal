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
                Temperature: { value: '', error: null },
                Humidity: { value: '', error: null },
                Precipitation: { value: '', error: null },
                BarometricPressure: { value: '', error: null },
                WindSpeed: { value: '', error: null },
                WindDirection: { value: '', error: null },
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
                        const temperature = response.data.data[length].temperature * 9 / 5 + 32;
                        const humidity = response.data.data[length].humidity;
                        const precipitation = response.data.data[length].precipitation / 25.4;
                        const barometricPressure = response.data.data[length].pressure ? response.data.data[length].pressure: 'UNAVAILABLE';
                        const windspeed = response.data.data[length].windspeed;
                        const windDirection = response.data.data[length].winddirection;
                        this.fields.WeatherSummary.value = `Temperature: ${temperature}F \n` +
                        `Humidity: ${humidity}% \n` +
                        `Precipitation: ${precipitation} inches \n` +
                        `Barometric Pressure: ${barometricPressure} hPa \n` +
                        `Wind Speed: ${windspeed/1.609} mph \n` +
                        `Wind Direction: ${windDirection} degrees \n`;
                        this.fields.Temperature.value = `${temperature}`;
                        this.fields.Humidity.value = `${humidity}`;
                        this.fields.Precipitation.value = `${precipitation}`;
                        this.fields.BarometricPressure.value = `${barometricPressure}`;
                        this.fields.WindSpeed.value = `${windspeed}`;
                        this.fields.WindDirection.value = `${windDirection}`;
                        this.retrievingWeather = false;
                    });
                });
        }
    }
});

new Vue({
    el: '#root'
});