#include <ESP8266WiFi.h>
#include <OneWire.h>
#include <DallasTemperature.h>
#include <SPI.h>
#include <SD.h>
 
#define ONE_WIRE_BUS D3
#define port 12727
#define MAXSC 2

OneWire oneWire(ONE_WIRE_BUS);
DallasTemperature sensors(&oneWire);
WiFiServer server(port);
WiFiClient *theAlat[MAXSC] = {new WiFiClient()} ;
WiFiClient checkClient;
File myFile;

const char* ssid="Tugas_Akhir";
const char* pass="123qweasd";

int numberOfDevices;
int x;
DeviceAddress tempDeviceAddress;

void setupWifi(){
  delay(100);
  
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
  Serial.print("Port : " + port);
  Serial.println("Server Started");
}

// function to print a sensor address
void printAddress(DeviceAddress deviceAddress) {
  
  for (uint8_t i = 0; i < 8; i++) {
    
    if (deviceAddress[i] < 16) 
      Serial.print("0");
      Serial.print(deviceAddress[i], HEX);
      
  }//End of for loop
  
}

void setup(void) {
  Serial.begin(115200);
  setupWifi();
  Serial.println("Initializing SD card...");
  if (!SD.begin(10)) {
  Serial.println("initialization SD CARD failed!");
  while (1);
  }
  else {
    myFile = SD.open("monitoring.txt", FILE_WRITE);
    myFile.println();
    myFile.println("Sensor1| Sensor2| Sensor3| Sensor4| Sensor5| Sensor6");
    Serial.println("Sensor1| Sensor2| Sensor3| Sensor4| Sensor5| Sensor6");
    myFile.close();
  }
  Serial.println("initialization done.");

  sensors.begin();
  // Get the number of sensors connected to the the wire( digital pin 4)
  numberOfDevices = sensors.getDeviceCount();
  
  Serial.print(numberOfDevices, DEC);
  Serial.println(" devices.");

  // Loop through each sensor and print out address
  for(int i=0; i<numberOfDevices; i++) {
    
    // Search the data wire for address and store the address in "tempDeviceAddress" variable
    if(sensors.getAddress(tempDeviceAddress, i)) {
      
      Serial.print("Found device ");
      Serial.print(i+1, DEC);
      Serial.print(" with address: ");
      printAddress(tempDeviceAddress);
      Serial.println();
      
    } else {
      
      Serial.print("Found ghost device at ");
      Serial.print(i, DEC);
      Serial.print(" but could not detect address. Check power and cabling");
     }
}
}

float tempC;
int i;

void loop(void) {
   myFile = SD.open("monitoring.txt", FILE_WRITE);
   sensors.requestTemperatures();

   // Loop through each device, print out temperature one by one
    for(i=0; i<numberOfDevices; i++) {
    
    // Search the wire for address and store the address in tempDeviceAddress
    if(sensors.getAddress(tempDeviceAddress, i)){
      tempC = sensors.getTempC(tempDeviceAddress); //Temperature in degree celsius

      if (i==0)
      { theAlat[x]->print("@" + String(tempC) + "#"); 
        myFile.print(String(tempC)+", ");
        Serial.print(String(tempC)+", ");}
      else if (i==1) 
      { theAlat[x]->print("$" + String(tempC) + "#"); 
        myFile.print(String(tempC)+", ");
        Serial.print(String(tempC)+", ");}
      else if (i==2) 
      { theAlat[x]->print("%" + String(tempC) + "#"); 
        myFile.print(String(tempC)+", ");
        Serial.print(String(tempC)+", ");}
      else if (i==3) 
      { theAlat[x]->print("&" + String(tempC) + "#"); 
        myFile.print(String(tempC)+", ");
        Serial.print(String(tempC)+", ");}
      else if (i==4) 
      { theAlat[x]->print("*" + String(tempC) + "#"); 
        myFile.print(String(tempC)+", ");
        Serial.print(String(tempC)+", ");}
      else if (i==5) 
      { theAlat[x]->println("!" + String(tempC) + "#"); 
        myFile.println(String(tempC));
        Serial.println(String(tempC));}
    }
      komunikasiTCP();
    }
      Serial.println();
      theAlat[x]->println();
      myFile.close();
      delay(1000);
}

void komunikasiTCP()
{
    checkClient = server.available();
    if(checkClient)
    {
      for(int x = 0; x < MAXSC; x++)
     {
      if(!theAlat[x]->connected())
      {
        theAlat[x] = new WiFiClient(checkClient);
        theAlat[x]->setNoDelay(true);
        Serial.print("New Alat Connected ");
        Serial.print(String(x+1) + " : ");
        Serial.println(theAlat[x]->remoteIP());            
        break;
      }
     }
    }
}
