using CoreLib.Blockchain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Wallet
{
    /// <summary>
    /// Interaction logic for BlockViewWindow.xaml
    /// </summary>
    public partial class BlockViewWindow : Window
    {
        public BlockViewWindow(Block block)
        {
            InitializeComponent();

            var Hash = Convert.ToBase64String(block.Hash);
            var PrevHash = block.PreviousHash!=null ? Convert.ToBase64String(block.PreviousHash): "null";
            var Merkle = Convert.ToBase64String(block.MerkleRoot);
            var index = block.Index;
            var transactions = block.Transactions;
            var nonce = block.Nonce;


            TreeViewItem h = new TreeViewItem() { Header = "Hash: " + Hash };
            TreeViewItem ph = new TreeViewItem() { Header = "PreviousHash: " + PrevHash };
            TreeViewItem mkl = new TreeViewItem() { Header = "Merkle root: " + Merkle };
            TreeViewItem i = new TreeViewItem() { Header = "Index: " + index };
            TreeViewItem n = new TreeViewItem() { Header = "Nonce: " + nonce };
            TreeViewItem txs = new TreeViewItem() { Header = "Transactions" };

            foreach (var tx in transactions)
            {
                var txId = Convert.ToBase64String(tx.Id);
                var t = new TreeViewItem() { Header = "Transaction: " + txId };
                
                var ins = new TreeViewItem() { Header = "Inputs"  };
                foreach (var item in tx.Inputs)
                {
                    var id = new TreeViewItem() { Header = "ID: " + (item.Id.Length != 0 ? Convert.ToBase64String(item.Id): "coinbase")};
                    var sub_out = new TreeViewItem() { Header = item.Out };                
                    var pk = new TreeViewItem() { Header = Convert.ToBase64String(item.PubKey) };                
                    var sign = new TreeViewItem() { Header = item.Signature != null ? Convert.ToBase64String(item.Signature): "null"};                
                    id.Items.Add(sub_out);
                    id.Items.Add(pk);
                    id.Items.Add(sign);

                    ins.Items.Add(id);
                }
                var ous = new TreeViewItem() { Header = "Outputs"  };
                foreach (var item in tx.Outputs)
                {
                    var id = new TreeViewItem() { Header = Convert.ToBase64String(item.PublicKeyHash) };                    
                    var value = new TreeViewItem() { Header = item.Value };
                    id.Items.Add(value);

                    ous.Items.Add(id);
                }


                t.Items.Add(ins);
                t.Items.Add(ous);

                txs.Items.Add(t);

            }

            Tree.Items.Add(h);
            Tree.Items.Add(ph);
            Tree.Items.Add(mkl);
            Tree.Items.Add(i);
            Tree.Items.Add(n);
            Tree.Items.Add(txs);




        }
    }
}
