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
        <div class="input-group">
          <button class="btn btn-info" v-on:click="getSecret">Get Secret</button>
          <input class="form-control" type="text" name="secret" placeholder="your-secret-here" v-model="settings.secret" />
        </div>
      </div>
      <div class="form-group">
        <label for="headerName">Header Name</label>
        <input class="form-control" type="text" name="headerName" placeholder="name for the header" v-model="settings.header" />
      </div>
      <div class="form-check">
        <input class="form-check-input" type="checkbox" id="includeWithPayload" v-model="settings.includeHeader" />
        <label class="form-check-label" id="includeWithPayload">include with payload</label>
      </div>
      <hr />
      <div class="form-group">
        <label for="callbackEndpoint">Callback Endpoint</label>
        <input class="form-control" type="text" name="callbackEndpoint" v-model="settings.callbackEndpoint" />
      </div>

      <h2>Create Webhook</h2>
      <h3>Validation Hash</h3>
      <div class="form-group">
        <label for="hash">HMAC SHA256</label>
        <input class="form-control" type="text" id="hash" v-model="computedHash" />
      </div>
      <h3>Payload</h3>
      <div class="form-group">
        <textarea class="form-control" id="payload" v-model="payload" rows="10"></textarea>
      </div>

      <div>
        <p><strong>Status Code</strong> {{ ui.responseStatusCode }}</p>
        <p><strong>Message</strong> {{ ui.responseMessage}}</p>

      </div>

      <button class="btn btn-primary" v-on:click="generateWebhook">Generate Webhook</button>
      <button class="btn btn-info" v-on:click="setValues">Set Values</button>

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

      <h2>Vue $data</h2>
      <pre>{{this.$data}}</pre>
    </template>
  </div>
</template>

<script>
  export default {
    data() {
      return {
        weatherEvents: null,
        ui: {
          apiResponse: "Click 'Generate Webhook' to submit.",
          responseStatusCode: "",
          responseMessage: ""
        },
        settings: {
          secret: "",
          includeHeader: true,
          header: "x-payload-sig",
          callbackEndpoint: "",
          getSecretEndpoint: "/api/callbacks/secret",
          defaultEndpoint: "/api/callbacks/weather"
        },
        payload: ""
      }
    },

    methods: {
      setValues() {
        const temp = {
          deviceId: Math.floor(Math.random() * 1000),
          temperature: Math.floor(Math.random() * 100),
          humidity: Math.floor(Math.random() * 100),
          barometer: Math.floor(Math.random() * 100),
          windspeed: Math.floor(Math.random() * 150),
          timestamp: new Date()
        };

        this.$set(this, 'payload', JSON.stringify(temp));
      },
      async getSecret() {
        let response = await this.$http.get(this.settings.getSecretEndpoint);
        this.settings.secret = response.data;
      },
      async generateWebhook() {
        try {
          //const p = JSON.parse(this.payload);
          let response = await this.$http.post(this.settings.callbackEndpoint, this.payload, this.submitHeaders);
          this.ui.apiResponse = response;
          this.ui.responseStatusCode = response.status;
          this.ui.responseMessage = response.data;

          this.weatherEvents.push(response.data);

          this.payload = "";
        }
        catch (err) {
          if (err) {
            this.ui.apiResponse = err;

            this.ui.responseStatusCode = err.response.status;
            this.ui.responseMessage = err.response.data;
          }
          else {
            this.ui.responseStatusCode = -1;
            this.ui.responseMessage = "unknown. possibly CORS/CORB.";
          }
        }
      },
      async loadPage(page) {
        // ES2017 async/await syntax via babel-plugin-transform-async-to-generator
        // TypeScript can also transpile async/await down to ES5
        this.currentPage = page

        try {
          // enable this to auto-retrieve the secret at page load
          //await this.getSecret();

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
            headers: {
              [this.settings.header]: this.computedHash,
              "Content-Type": "application/json"
            }
          }
        }
        return extra;
      },
      "computedHash": function () {
        //const p = JSON.stringify(this.payload);
        
        const sha = new this.$jssha("SHA-256", "TEXT");
        sha.setHMACKey(this.settings.secret, "TEXT");
        sha.update(this.payload);

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
      this.loadPage(1);

      this.settings.callbackEndpoint = `${window.location.origin}${this.settings.defaultEndpoint}`;

      //console.log(window.location);
    }
  }
</script>

<style>
</style>
