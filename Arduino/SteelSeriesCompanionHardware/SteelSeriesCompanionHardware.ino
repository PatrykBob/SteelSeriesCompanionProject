void setup() 
{
  Serial.begin(9600);
}

void loop() 
{
  delay(100);

  int value1 = analogRead(A0);
  int value2 = analogRead(A1);
  int value3 = analogRead(A2);

  Serial.print(value1);
  Serial.print("A");
  Serial.print(value2);
  Serial.print("B");
  Serial.print(value3);
  Serial.print("C");
  Serial.print("\n");
}
