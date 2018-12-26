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
        private Button btnUpload, btnChoose, btnCapture;
        private ImageView imgView;
        public Bitmap mBitMap;
        private Android.Net.Uri filePath;
        private const int PICK_IMAGE_REQUSET = 71;
        private const int TAKE_IMAGE_REQUSET = 0;
        private EditText edtURL;
        private MemoryStream inputStream;
        private ProgressBar progressBar;
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
            progressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);

            //Events
            btnChoose.Click += delegate
            {
                ChooseImage();
            };

            btnCapture.Click += delegate
            {
                CaptureImage();
            };

            btnUpload.Click += delegate
            {
                UploadImage();
                Busy();
            };
        }

        private void CaptureImage()
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            StartActivityForResult(intent, 0);
        }

        private void UploadImage()
        {
            if (inputStream != null)
                Upload(inputStream);
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
                    mBitMap = MediaStore.Images.Media.GetBitmap(ContentResolver, filePath);
                    imgView.SetImageBitmap(mBitMap);
                    byte[] bitmapData;
                    using (var stream = new MemoryStream())
                    {
                        mBitMap.Compress(Bitmap.CompressFormat.Jpeg, 100, stream);
                        bitmapData = stream.ToArray();
                    }
                    inputStream = new MemoryStream(bitmapData);
                }
                catch (IOException ex)
                {
                    Console.WriteLine(ex);
                }
            }
            else if (requestCode == 0 &&
                resultCode == Result.Ok &&
                data != null)
            {
                try
                {
                    mBitMap = (Bitmap)data.Extras.Get("data");
                    imgView.SetImageBitmap(mBitMap);
                    byte[] bitmapData;
                    using (var stream = new MemoryStream())
                    {
                        mBitMap.Compress(Bitmap.CompressFormat.Jpeg, 100, stream);
                        bitmapData = stream.ToArray();
                    }
                    inputStream = new MemoryStream(bitmapData);
                }
                catch (IOException ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        //Upload to blob function
        private async void Upload(Stream stream)
        {
            try
            {
                var account = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=ahsanblob;AccountKey=fOvpvzb8jFL0pNfDWvz9n76DzLWSlZu4aw6ZLXMbDId15YYfox15UoKvWMmTCJ6vcNoyk5wBhQu0V1LSg0Qw+A==;EndpointSuffix=core.windows.net");
                var client = account.CreateCloudBlobClient();
                var container = client.GetContainerReference("images");
                await container.CreateIfNotExistsAsync();
                var name = Guid.NewGuid().ToString();
                var blockBlob = container.GetBlockBlobReference($"{name}.png");
                await blockBlob.UploadFromStreamAsync(stream);
                URL = blockBlob.Uri.OriginalString;
                edtURL.Text = URL;
                Toast.MakeText(this, "Image uploaded to Blob Storage Successfully!", ToastLength.Short).Show();
                NotBusy();
            }
            catch (Exception e)
            {
                Toast.MakeText(this, "" + e.ToString(), ToastLength.Short);
            }
        }

        void Busy()
        {
            btnCapture.Enabled = false;
            btnChoose.Enabled = false;
            btnUpload.Enabled = false;
            progressBar.Visibility = Android.Views.ViewStates.Visible;
        }
        void NotBusy()
        {
            btnCapture.Enabled = true;
            btnChoose.Enabled = true;
            btnUpload.Enabled = true;
            progressBar.Visibility = Android.Views.ViewStates.Invisible;
        }
    }
}