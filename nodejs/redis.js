import * as redis from 'redis'

const client = redis.createClient({
  url: 'redis://localhost:6379'
});

client.connect();

client.on('connect', () => {
  console.log('✅ Conectado a Redis');
});

client.on('error', (err) => {
  console.error('❌ Error en Redis:', err);
});

const pingValue = await client.ping();
console.log('Ping:', pingValue);

await client.set('greeting', 'Hola Juan');
const greetingValue = await client.get('greeting');
console.log('Greeting:', greetingValue);



