bytes_s = b"\x00\x00\x02\x01"

a = bytes_s[-3:-1]
b = bytes_s[:-2]

print(bytes_s)
print(a)
print(len(b))