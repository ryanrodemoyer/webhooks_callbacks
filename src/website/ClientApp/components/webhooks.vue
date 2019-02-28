<template>
  <div>

    <div v-if="!weatherEvents" class="text-center">
      <p><em>Loading...</em></p>
      <h1><icon icon="spinner" pulse /></h1>
    </div>

    <template v-if="weatherEvents">
      <h1>Weather Webhooks</h1>
      <h2>Settings</h2>
      <div class="form-group">
        <label for="secret">Shared Secret</label>
        <input class="form-control" type="text" name="secret" placeholder="your-secret-here" v-model="settings.secret" />
      </div>
      <div class="form-check">
        <input class="form-check-input" type="checkbox" id="includeWithPayload" v-model="settings.includeHeader" />
        <label class="form-check-label" id="includeWithPayload">include with payload</label>
      </div>
      <div class="form-group">
        <label for="headerName">Header Name</label>
        <input class="form-control" type="text" name="headerName" placeholder="name for the header" v-model="settings.header" />
      </div>

      <h2>Create Webhook</h2>
      <h3>Validation Hash</h3>
      <div class="form-group">
        <label for="hash">HMAC SHA256</label>
        <input class="form-control" type="text" id="hash" v-model="computedHash" />
      </div>
      <h3>Payload</h3>
      <div class="form-group">
        <label>Device Id</label>
        <input class="form-control" type="number" id="deviceId" v-model="payload.deviceId" />
      </div>
      <div class="form-group">
        <label>Temperature</label>
        <input class="form-control" type="number" id="temperature" v-model="payload.temperature" />
      </div>
      <div class="form-group">
        <label>Humidity</label>
        <input class="form-control" type="number" id="humidity" v-model="payload.humidity" />
      </div>
      <div class="form-group">
        <label>Barometer</label>
        <input class="form-control" type="number" id="barometer" v-model="payload.barometer" />
      </div>
      <div class="form-group">
        <label>Windspeed</label>
        <input class="form-control" type="number" id="windspeed" v-model="payload.windspeed" />
      </div>

      <button class="btn btn-primary" v-on:click="generateWebhook">Generate Webhook</button>
      <button class="btn btn-info" v-on:click="setValues">Set Values</button>

      <!--<h3>Response</h3>
  <pre style="background-color: grey; border: solid 1px black; padding: 5px;">{{ui.apiResponse}}</pre>-->

      <h2>Received Webhooks</h2>

      <table class="table">
        <thead class="bg-dark text-white">
          <tr>
            <th>Id</th>
            <th>Date</th>
            <th>Device Id</th>
            <th>Temperature</th>
            <th>Humidity</th>
            <th>Barometer</th>
            <th>Windspeed</th>
          </tr>
        </thead>
        <tbody>
          <tr :class="index % 2 == 0 ? 'bg-white' : 'bg-light'" v-for="(we, index) in weatherEvents" :key="we.id">
            <td>{{ we.id }}</td>
            <td>{{ we.timestamp }}</td>
            <td>{{ we.deviceId }}</td>
            <td>{{ we.temperature }}</td>
            <td>{{ we.humidity }}</td>
            <td>{{ we.barometer }}</td>
            <td>{{ we.windspeed }}</td>
          </tr>
        </tbody>
      </table>

      <pre>{{this.$data}}</pre>
    </template>
  </div>
</template>

<script>
  export default {
    computed: {
    },

    data() {
      return {
        weatherEvents: null,
        ui: {
          apiResponse: "Click 'Generate Webhook' to submit.",
        },
        settings: {
          secret: "MaryHadALittleLambLittleLamb",
          includeHeader: true,
          header: "x-payload-sig"
        },
        payload: {
          //deviceId: null,
          //temperature: 17,
          //humidity: 87,
          //barometer: 29,
          //windspeed: 3,
          timestamp: new Date()
        }
      }
    },

    methods: {
      setValues() {
        this.$set(this.payload, 'deviceId', Math.floor(Math.random() * 1000));
        this.payload.temperature = Math.floor(Math.random() * 100);
        this.payload.humidity = Math.floor(Math.random() * 100);
        this.payload.barometer = Math.floor(Math.random() * 100);  
        this.payload.windspeed = Math.floor(Math.random() * 150);
        this.timestamp = new Date();
      },
      async generateWebhook() {
        try {
          let response = await this.$http.post(`/api/callbacks/weather`, this.payload, this.submitHeaders);
          this.ui.apiResponse = response;

          this.weatherEvents.push(response.data);

          this.payload = {}
          this.$set(this.payload, 'timestamp', new Date());
        }
        catch (err) {
          this.ui.apiResponse = JSON.stringify(err, null, 2);
        }
      },
      async loadPage(page) {
        // ES2017 async/await syntax via babel-plugin-transform-async-to-generator
        // TypeScript can also transpile async/await down to ES5
        this.currentPage = page

        try {
          let response = await this.$http.get(`/api/weatherreadings`);
          //console.log(response.data);
          this.weatherEvents = response.data;


        } catch (err) {
          window.alert(err)
          console.log(err)
        }
      }
    },

    computed: {
      "submitHeaders": function () {
        let extra = null;
        if (this.settings.includeHeader) {
          extra = {
            headers: { [this.settings.header]: this.computedHash }
          }
        }
        return extra;
      },
      "computedHash": function () {
        const p = JSON.stringify(this.payload);
        
        const sha = new this.$jssha("SHA-256", "TEXT");
        sha.setHMACKey(this.settings.secret, "TEXT");
        sha.update(p);

        let encoded = sha.getHMAC("B64");
        return encoded;
      }
    },

    //watch: {
    //  "payload.temperature": function(curr, prev) {
        
    //  }
    //},

    //mounted() {
    //  console.log("here in mounted");
    //  //console.log(`curr=${curr}`);

    //  setTimeout(() => {

    //  }, 3000);

    //},

    async created() {
      this.loadPage(1)
    }
  }
</script>

<style>
</style>
