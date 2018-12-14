using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Widget;
using Microsoft.WindowsAzure.Storage;
using System;
using System.IO;

namespace UploadToBlob.Droid
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private Button btnUpload, btnChoose , btnCapture;
        private ImageView imgView;
        private EditText edtURL;
        private Android.Net.Uri filePath;
        private const int PICK_IMAGE_REQUSET = 71;

        public string URL { get; private set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            btnChoose = FindViewById<Button>(Resource.Id.btnChoose);
            btnUpload = FindViewById<Button>(Resource.Id.btnUpload);
            btnCapture = FindViewById<Button>(Resource.Id.btnCapture);
            imgView = FindViewById<ImageView>(Resource.Id.imgView);
            edtURL = FindViewById<EditText>(Resource.Id.edtURL);
            //Events  
            btnChoose.Click += delegate
            {
                ChooseImage();
            };
            btnUpload.Click += delegate 
            {

            };
        }
        private void ChooseImage()
        {
            Intent intent = new Intent();
            intent.SetType("image/*");
            intent.SetAction(Intent.ActionGetContent);
            StartActivityForResult(Intent.CreateChooser(intent, "Select Picture"), PICK_IMAGE_REQUSET);
        }
        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == PICK_IMAGE_REQUSET &&
                resultCode == Result.Ok &&
                data != null &&
                data.Data != null)
            {
                filePath = data.Data;
                try
                {
                    Bitmap bitmap = MediaStore.Images.Media.GetBitmap(ContentResolver, filePath);
                    imgView.SetImageBitmap(bitmap);
                }
                catch (IOException ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        private async void Upload(Stream stream)
        {
            var account = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=blobacount;AccountKey=hMZuDKfdz1iGPVsV+W9V52YnsjD6F1tjdH89XIw0QM3J6FB+tdJ9IgQI+OAWHgCRKSYMK0EwGcpB0qCJI8kY+w==;EndpointSuffix=core.windows.net");
            var client = account.CreateCloudBlobClient();
            var container = client.GetContainerReference("images");
            await container.CreateIfNotExistsAsync();
            var name = Guid.NewGuid().ToString();
            var blockBlob = container.GetBlockBlobReference($"{name}.png");
            await blockBlob.UploadFromStreamAsync(stream);
            URL = blockBlob.Uri.OriginalString;
            edtURL.Text = URL;
            Toast.MakeText(this, "Image uploaded to Blob Storage Successfully", ToastLength.Short).Show();
        }
    }
}