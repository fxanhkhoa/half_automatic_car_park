from filehash import FileHash

md5hasher = FileHash('md5')
file1 = md5hasher.hash_file("../0000_00532_b.jpg")
file2 = md5hasher.hash_file("../images/0000_00532_b.jpg")

print(file1)
print(file2)