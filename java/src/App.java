public class App {
	public static void main(String[] args) throws Exception {
		Jedis jedis = new Jedis("localhost", 6379);
		jedis.set("my-key", "my value on my-key key");
		System.out.println(jedis.get("my-key"));
		jedis.close();
	}
}
