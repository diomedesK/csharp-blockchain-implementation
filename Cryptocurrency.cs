using System;
using System.Security.Cryptography;

namespace Cryptocurrency
{
    class Algorithms
    {
        static public List<List<byte[]>> BuildMerkleTree( byte[][] transactions ){

            using (SHA256 sha256 = SHA256.Create())
            {
                if( transactions.Count() == 1 ){
                    /* if there is only one transaction, then that transaction in its raw form is the root */
                    return new List<List<byte[]>>{  new List<byte[]>{transactions.Last()} };
                }

                List<List<byte[]>> layers = new List<List<byte[]>> { 
                    /*a little-endianness is required to perfectly match bitcoin's merkle tree */
                    transactions.Select( (data) => data.Reverse().ToArray() ).ToList()
                };

                while( layers.Last().Count > 1 ){
                    List<byte[]> hashes = layers.Last();

                    if ( hashes.Count() % 2 != 0 ){
                        hashes.Add( hashes.Last() );
                    }

                    List<byte[]> onelayerabov = hashes
                        .Where( (_, idx) => idx % 2 == 0 )
                        .Zip( hashes.Where( (_, idx) => idx % 2 != 0 ), Tuple.Create )
                        .Select( (tup) => ( sha256.ComputeHash( sha256.ComputeHash(  tup.Item1.Concat(tup.Item2).ToArray()  )) ) )
                        .ToList();

                    layers.Add( onelayerabov );
                }

                /* again, little-endianness: the root value is also required to be in little-endian notation */
                layers[^1][^1] = layers[^1][^1].Reverse().ToArray();
                return layers;
            }
        }
    }

    public struct Block {
        /*
           The block header is defined as follows:
           VERSION         4       int32
           PREVIOUS_HASH   32      char[32]
           MERKLE_ROOT     32      char[32]
           TIME (EPOCH)    4       uint32
           BITS (TARGET)   4       uint32
           NONCE           4       uint32
           */

        public int version {get; private set;}
        public byte[] previoushash { get; }
        public byte[] merkleroot { get; }
        public uint timestamp{ get; }
        public uint target{ get; }
        public uint nonce { get; set; }

        private byte[] _fixedheader;
        public double _maximumvalue { get; }

        public double difficulty { get;  }

        public static Func<uint, double> get_maximum_value = (val) => ( (val & 0xffffff) ) * (Math.Pow(2, ( 8 * ( (val >> (3 * 8)) - 3)) ));
        public static double MAX_TARGET = Block.get_maximum_value(0x1d00ffff);

        public Block( int version, byte[] previoushash, byte[] merkleroot, uint timestamp, uint target){
            this.version = version;
            this.previoushash = previoushash;
            this.merkleroot = merkleroot;
            this.timestamp = timestamp;
            this.target = target;

            this._maximumvalue = get_maximum_value(target);
            this.difficulty = MAX_TARGET / this._maximumvalue;

            using ( MemoryStream ms = new MemoryStream() )
                using ( BinaryWriter bw  = new BinaryWriter(ms) )
                {
                    bw.Write(version);
                    bw.Write(previoushash);
                    bw.Write(merkleroot);
                    bw.Write(timestamp);
                    bw.Write(target);

                    this._fixedheader = ms.ToArray();
                }
        }

        public Block( int version, byte[] previoushash, byte[] merkleroot, uint timestamp, uint target, uint nonce)
            : this(version, previoushash, merkleroot, timestamp, target)
        {
            this.nonce = nonce;
        }

        public bool is_valid(){
            return BitConverter.ToDouble( this.hash ) < this._maximumvalue;
        }

        public byte[] header{ get {
            // bw is little-endian by default
            using ( MemoryStream ms = new MemoryStream() )
                using ( BinaryWriter bw  = new BinaryWriter(ms) )
                {
                    bw.Write(this._fixedheader);
                    bw.Write(nonce);

                    return ms.ToArray();
                }
        }}

        public byte[] hash{ get{
            using (SHA256 sha256 = SHA256.Create()){
                return sha256.ComputeHash( sha256.ComputeHash( this.header ) ).Reverse().ToArray();
            }
        }}
    }

    public interface IBlockchain{
        public bool Add( Block blk );
    }

    public class Blockchain : IBlockchain {
        private List<Block> _chain;
        
        public uint target { get;  }
        public int version { get; }

        /* i make an oath with myself to make a real blockchain with tree structure one day */
        public List<Block> chain{ 
            get{
                return _chain.ToArray().ToList(); // makes a copy instead of providing the reference
            }
            private set {}
        }

        public Blockchain(){
            this._chain = new List<Block>();
            this.target = 0x1d00ffff;
            this.version = 0x1;

            Block genesis = new Block(
                    /* actually this is the block of height 1 in the Blockchain, not 0, but it serves */
                    this.version,
                    Convert.FromHexString("000000000019d6689c085ae165831e934ff763ae46a2a6c172b3f1b60a8ce26f").Reverse().ToArray(),
                    Convert.FromHexString("0e3e2357e806b6cdb1f70b54c3a3a17b6714ee1f0e68bebb44a74b1efd512098").Reverse().ToArray(),
                    1231469665, //hardcoded timestamp
                    this.target,
                    2573394689 
                    );

            this._chain.Add(genesis);
        }

        public Block Last { 
            get {
                return this._chain.Last();
            } 
            private set{}
        }

        public bool Add( Block blk ){
            if ( 
                    blk.target == this.target &&
                    blk.version == this.version &&
                    _chain.Last().hash == blk.previoushash && 
                    blk.is_valid() == true 
               ){
                _chain.Add(blk);

                updateTarget();
                return false;
            }

            return false;
        }

        private void updateTarget(){
            // TODO
        }

    }

}
