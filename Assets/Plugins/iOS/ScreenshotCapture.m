#import <Photos/Photos.h>

bool CheckPermissions()
{
    PHAuthorizationStatus authorizationStatus = [PHPhotoLibrary authorizationStatus];
    return authorizationStatus == PHAuthorizationStatusAuthorized;
}

void AskPermissions()
{
    [PHPhotoLibrary requestAuthorization:^(PHAuthorizationStatus status) {}];
}

void SaveScreenshotToAlbum(const char* path)
{
	UIImage *image = [UIImage imageWithContentsOfFile:[NSString stringWithUTF8String:path]];

    if (image == nil) NSLog(@"Image is nil at path %@", [NSString stringWithUTF8String:path]);

    [[PHPhotoLibrary sharedPhotoLibrary] performChanges:
    ^{
        [PHAssetChangeRequest creationRequestForAssetFromImage:image];
    } completionHandler:^(BOOL success, NSError * _Nullable error)
    {
        if (success) NSLog(@"SAVED!");
        else
        {
            NSLog(@"%@", error.description);
        }
    }];
}
