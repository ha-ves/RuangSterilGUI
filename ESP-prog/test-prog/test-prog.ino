#include <WiFi.h>

WiFiServer server(12727);
WiFiClient theAlat = WiFiClient();

const char* ssid = "Test_ESP";
const char* pass = "12345678";

void setup() {
  // put your setup code here, to run once:
  WiFi.mode(WIFI_AP);
  WiFi.softAP(ssid, pass);
  Serial.print(" ");
  Serial.println("WIFI  " + String(ssid) + "  ... Started");
  
  // Getting Server IP
  IPAddress IP = WiFi.softAPIP();
  
  // Printing The Server IP Address
  Serial.print("AccessPoint IP : ");
  Serial.println(IP);
  
  server.begin();
  Serial.println("Port : " + String(12727));
  Serial.println("Server Started");
}

float tempC = M_PI * 10.0f + 50.0f;
long lastmil = 0;
void loop() {
  // put your main code here, to run repeatedly:
  if(theAlat.connected() && millis()-lastmil > 1000){
    lastmil = millis();
    for(int i=0; i<6; i++){
      if (i==0)
      { theAlat.print("@" + String(tempC) + "#");
        Serial.print(String(tempC)+", ");}
      else if (i==1) 
      { theAlat.print("$" + String(tempC) + "#");
        Serial.print(String(tempC)+", ");}
      else if (i==2) 
      { theAlat.print("%" + String(tempC) + "#");
        Serial.print(String(tempC)+", ");}
      else if (i==3) 
      { theAlat.print("&" + String(tempC) + "#");
        Serial.print(String(tempC)+", ");}
      else if (i==4) 
      { theAlat.print("*" + String(tempC) + "#");
        Serial.print(String(tempC)+", ");}
      else if (i==5) 
      { theAlat.print("!" + String(tempC) + "#");
        Serial.println(String(tempC));}
    }
  }
  else{
    WiFiClient checkClient = server.available();
    if(checkClient){
      theAlat = WiFiClient(checkClient);
      theAlat.setNoDelay(true);
      Serial.print("New Alat Connected ");
      Serial.println(theAlat.remoteIP()); 
    }
  }
}
