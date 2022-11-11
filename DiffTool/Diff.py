

originalNameList = r"E:\11-23-2021 Ps2Test Resources\NameLookup.txt"
originalDict = {}
for line in open(originalNameList, "r").read().splitlines(): #expect data in hashxxxxxxx:name_name_name.name format
    temp = line.split(':')
    originalDict[temp[0]] = temp[1]
print("Original has " + str(len(originalDict)) + " names.")

newNameList = r"C:\Users\Nathan\Desktop\Projects\dbg-pack\NameLookup.txt"
newDict = {}
for line in open(newNameList, "r").read().splitlines(): #expect data in hashxxxxxxx:name_name_name.name format
    temp = line.split(':')
    newDict[temp[0]] = temp[1]
print("New has " + str(len(newDict)) + " names.")

differenceFile = r"Diff.txt"
with open(differenceFile, "w") as text_file:
    text_file.write("-------------------------------------\n")
    text_file.write("------------New Assets---------------\n")
    text_file.write("-------------------------------------\n")
    for key in newDict.keys():
        if key not in originalDict:
            text_file.write(newDict[key] + "\n")
            
    text_file.write("-------------------------------------\n")
    text_file.write("------------Lost Assets--------------\n")
    text_file.write("-------------------------------------\n")
    for key in originalDict.keys():
        if key not in newDict:
            text_file.write(originalDict[key] + "\n")
            
print("Done")