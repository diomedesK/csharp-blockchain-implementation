using Cryptocurrency;
using System.Numerics;

internal class Program {
    static Random rand = new Random();

    public static string stringify( byte[] bytes ){
        return BitConverter.ToString(bytes).Replace("-", "").ToLower();
    }

    public static void Pain(String[] args){
        Blockchain bc = new Blockchain();
        Random rnd = new Random();

        while( true ){
            byte[][] transactions = new byte[]{ (byte) rnd.Next(), (byte) rnd.Next() }.Select( (el) => new byte[]{el} ).ToArray();
            byte[] merkleroot = Cryptocurrency.Algorithms.BuildMerkleTree(transactions)[^1][^1];
            byte[] previoushash = bc.Last.hash;
            int version = bc.version;
            uint target = bc.target;
            uint timestamp = (uint) DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            Block blk = new Block( version, previoushash, merkleroot, timestamp, target );

            for ( uint nonce = 0 ; !blk.is_valid(); nonce++ ){
                blk.nonce = nonce;
            }

            Console.WriteLine("Mined a new block with the nonce :D {0} | {1} | {2}", blk.nonce, blk._maximumvalue, BitConverter.ToDouble( blk.hash ));
            Console.WriteLine("{0}", stringify(blk.hash));

            Console.ReadLine();
        }

    }

    /*
       idx  cof
       0x1d 0x00ffff

       0x0001   ^0
       0x0010   ^1
       0x0100   ^2
       0x1000   ^3
    */


    public static void Main(String[] args){
        byte[] root = Convert.FromHexString("2b12fcf1b09288fcaff797d71e950e71ae42b91e8bdb2304758dfcffc2b620e3").Reverse().ToArray();
        byte[] prev = Convert.FromHexString("00000000000008a3a41b85b8b29ad444def299fee21793cd8b9e567eab02cd81").Reverse().ToArray();
        Block blk = new Block(0x1, prev, root, 1305998791, 0x1a44b9f2);
        blk.nonce = 2504433986;

        // 0x00000000ffff0000000000000000000000000000000000000000000000000000
        BigInteger bi = new BigInteger((0x1 << (8 * (0x1d - 0x3)))) * 0x0ffff;
        Console.WriteLine(string.Join(" ", bi.ToByteArray()));

        Console.WriteLine(string.Join(" ", BigInteger.Parse("26959535291011309493156476344723991336010898738574164086137773096960").ToByteArray()));

        Console.WriteLine( string.Join(" ", blk.hash) );

    }

}

