/* Slave (köle) görevindeki Arduino'nun kodu */
#include <Wire.h>
/* 
 * I2C fonksiyonlarını kullanabilmek için 
 * Wire.h kütüphanesini projemize ekledik
 */
 
 const int LED = 13;
 /* LED 13. pinde bulunmaktadır */
 
void setup()
{

 Serial.begin(9600);
  randomSeed(analogRead(0));
  Wire.begin(1);
  /* I2C haberleşmesi, haberleşme adresi 1 olan bir slave cihaz olarak başlatıldı */
  Wire.onRequest(istekGeldiginde);
  /* 
  Master olan cihaz bu Arduino'dan veri istediğinde gerçekleşecek işlem seçildi
  */
  Wire.onReceive(veriGeldiginde);
  /*
  Master olan cihazdan bu Arduino'ya veri geldiğinde yapılacak işlem seçildi
  */
  
  pinMode(LED,OUTPUT);
  /* LED pini çıkış olarak ayarlandı */
}
 
void loop()
{
  /*
  * Tüm işlemler veri isteği geldiğinde veya yeni veri geldiğinde 
  * yapılacağı için loop fonksiyonunun içi boş bırakılmıştır
  */
  delay(1);
}
 
void veriGeldiginde(int veri)
{
  /* I2C hattında bu cihaz için yeni veri olduğunda bu fonksiyon çalışır */
  char gelenKarakter;
  /* Hattaki veri okunarak gelenKarakter değişkenine kaydedilir */
  while(Wire.available()){
    gelenKarakter = Wire.read();
  }
  /* Eğer gelen veri 'a' ise LED yakılır, 'b' ise LED söndürülür */
  if(gelenKarakter == 'a')
    digitalWrite(LED,HIGH);
  else if(gelenKarakter == 'b')
    digitalWrite(LED,LOW);
}

String generateValues(int deviceid)
{
  float usage_raw = random(1000)/10.0f;
  float freeusage = random(1000)/30.0f;
  float usage = freeusage+ usage_raw;
  String s = "";
  s.concat(deviceid); s.concat(',');
  s.concat(usage); s.concat(',');
  s.concat(freeusage); s.concat(';');
  return s;
}
 
void istekGeldiginde()
{
  /* 
  * Eğer master bu cihazdan veri istiyor ise master cihaza "Merhaba" verisi yollanılır 
  * Eğer bu bir sensör olsaydı "merhaba" yerine sıcaklık veya ivme verisi yollanıyor olacaktı
  */

  auto s = generateValues(1);
  // herhangi bir sinir asimi olmamasi icin 32 bitlik bir alan reserve ediyoruz
  s.reserve(32);
  Wire.write(s.c_str() , 32);

}
