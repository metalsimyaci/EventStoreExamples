# EventStoreExample

Event Store kullanımın anlaşılması için [Ahmet KÜÇÜKOĞLU](https://www.ahmetkucukoglu.com/asp-net-core-ile-event-sourcing-01-store/) beyin makalesi baz alınarak hazırlanan uygulama çalışması.


## Event Store' un Docker üzerinden ayağa kaldırılması

``` bash

docker run --name eventstore-node -it -p 2113:2113 -p 1113:1113 eventstore/eventstore:release-5.0.9
```

ile kurulum yaptığınızda 

``EventStore.ClientAPI.Exceptions.RetriesLimitReachedException: Item Operation ReadStreamEventsForwardOperation`` 
şeklinde bir hata alıyorsunuz. Bunun çözümüde [Gençay YILDIZ](https://www.gencayyildiz.com/blog/net-core-ortaminda-event-store-ile-event-sourcing-yapilanmasi/) beyin belirttiği gibi ssl konfigurasyonu ile ayağa kaldırmak.

bunun için src içerisinde hazır bir ``dockerfile`` bulunmakta.

``cmd`` üzerinde src klasörü içerinde yer alan ``dockerfile`` olduğu dizine gidiyoruz.

```bash

docker build -t eventstore/eventstore:with-cert-local --no-cache .
```
komutu ile ``image`` ımızı oluşturuyoruz.

```bash
docker run --name EventStore -it -p 11113:1113 -p 11115:1115 -p 21113:2113 -e EVENTSTORE_CERTIFICATE_FILE=eventstore.p12 -e EVENTSTORE_EXT_SECURE_TCP_PORT=1115 eventstore/eventstore:with-cert-local
```

> 💡 Bende 4 haneli portlar kapalı olduğu için portları kendime göre düzenledim. siz direkt 1113, 1115, 2113 portlarını açabilirsiniz.

Event Store container’ı ayağa kaldırdıktan sonra http://localhost:21113/ adresi üzerinden Admin UI’a erişebilirsiniz. Kullanıcı adı **admin**, şifre **changeit**‘tir.

Bağlantı bilgisini aşağıdaki gibi set edebiliyoruz.

```json
ConnectTo=tcp://localhost:11115;DefaultUserCredentials=admin:changeit;UseSslConnection=true;TargetHost=eventstore.org;ValidateServer=false
```

## Couchbase' i docker üzerinden ayağa kaldırma

``` bash
docker run -d --name couchbase -p 8091-8094:8091-8094 -p 11210:11210 couchbase
```

Couchbase ayağa kalktığında aşağıdaki adresten panele girip bir cluster oluşturmalısınız.

http://localhost:8091/

Aşağıdaki curl komut satırları ile Couchbase’de “checkpoints” ve “tasks” isimli iki adet bucket oluşturalım. -u parametresini cluster oluştururken vermiş olduğunuz bilgiler ile değiştirmeyi unutmayın.

```bash
curl -X POST -u [admin]:[password] http://localhost:8091/pools/default/buckets
  -d name=checkpoints -d ramQuotaMB=100 -d authType=none -d bucketType=couchbase
```

```bash
curl -X POST -u [admin]:[password] http://localhost:8091/pools/default/buckets
  -d name=tasks -d ramQuotaMB=100 -d authType=none -d bucketType=couchbas
```

### Kaynak

[Gençay YILDIZ](https://www.gencayyildiz.com/blog/net-core-ortaminda-event-store-ile-event-sourcing-yapilanmasi/)
[Ahmet KÜÇÜKOĞLU](https://www.ahmetkucukoglu.com/asp-net-core-ile-event-sourcing-01-store/)