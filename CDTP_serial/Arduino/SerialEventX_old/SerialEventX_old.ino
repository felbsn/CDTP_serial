 

String inputString = "";          
bool stringComplete = false;   

void setup() {
  // initialize serial:
  Serial.begin(9600);
 
  //Serial.write("begin...");
  inputString.reserve(200);
}

void loop() {
  // print the string when a newline arrives:
  if (stringComplete) {

    if(inputString.startsWith("getValues"))
    {   
      String str= "";     
      for(int i = 0 ; i < 3; i++)
      {     
        float usage = random(1000) /10.0;
        float freeusage = random(1000) /10.0;
        str.concat(i);
        str.concat(",");
        str.concat(usage);
        str.concat(",");
        str.concat(freeusage);
        str.concat(";");
      }
      
      Serial.println(str.c_str());
    }else
    {
        Serial.print("given string:");
        Serial.println(inputString.c_str());
    }


    // clear the string:
    inputString = "";
    stringComplete = false;
  }
}

void serialEvent() {
  while (Serial.available()) {
    // get the new byte:
    char inChar = (char)Serial.read();
    // add it to the inputString:
    inputString += inChar;
    // if the incoming character is a newline, set a flag so the main loop can
    // do something about it:
    if (inChar == '\n') {
      stringComplete = true;
    }
  }
}
