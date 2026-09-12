#include <WiFi.h>
#include <HTTPClient.h>
#include <DHT.h>

#include "secrets.h"

constexpr char DEVICE_ID[] = "esp32-s3-lab-01";

constexpr int DHT_PIN = 4;
constexpr int DHT_TYPE = DHT11;
constexpr unsigned long TELEMETRY_INTERVAL_MS = 5000;

DHT dht(DHT_PIN, DHT_TYPE);

unsigned long lastTelemetryTime = 0;

void connectToWiFi() {
  WiFi.mode(WIFI_STA);
  WiFi.begin(WIFI_SSID, WIFI_PASSWORD);

  Serial.print("Connecting to Wi-Fi");

  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }

  Serial.println();
  Serial.print("Wi-Fi connected. ESP32 IP: ");
  Serial.println(WiFi.localIP());
}

void sendTelemetry() {
  float humidity = dht.readHumidity();
  float temperature = dht.readTemperature();

  if (isnan(humidity) || isnan(temperature)) {
    Serial.println("DHT11 read failed. Telemetry was not sent.");
    return;
  }

  if (WiFi.status() != WL_CONNECTED) {
    connectToWiFi();
  }

  String payload =
    String("{\"deviceId\":\"") + DEVICE_ID +
    "\",\"voltage\":0" +
    ",\"current\":0" +
    ",\"powerWatts\":0" +
    ",\"temperatureCelsius\":" + String(temperature, 1) +
    ",\"humidityPercent\":" + String(humidity, 1) +
    "}";

  HTTPClient http;
  String endpoint = String(API_BASE_URL) + "/api/readings";

  http.begin(endpoint);
  http.addHeader("Content-Type", "application/json");

  int responseCode = http.POST(payload);

  Serial.print("POST ");
  Serial.print(endpoint);
  Serial.print(" | HTTP ");
  Serial.println(responseCode);

  if (responseCode > 0) {
    Serial.println(http.getString());
  }

  http.end();
}

void setup() {
  Serial.begin(115200);
  delay(1000);

  dht.begin();
  connectToWiFi();

  Serial.println("PowerPulse ESP32 telemetry started.");
}

void loop() {
  if (millis() - lastTelemetryTime >= TELEMETRY_INTERVAL_MS) {
    lastTelemetryTime = millis();
    sendTelemetry();
  }
}