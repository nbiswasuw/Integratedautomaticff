import ftplib
session = ftplib.FTP('10.0.0.6','ffws','ffws*iwm')
session.cwd("/html/images")

file = open('E:\FFWS\ModelOutput\StructureFF\Dhaka-Mawa.png','rb')
session.storbinary('STOR Dhaka-Mawa.png', file)
file.close()

file = open('E:\FFWS\ModelOutput\StructureFF\Jamuna-RB.png','rb')
session.storbinary('STOR Jamuna-RB.png', file)
file.close()

file = open('E:\FFWS\ModelOutput\StructureFF\MDIP.png','rb')
session.storbinary('STOR MDIP.png', file)
file.close()

file = open('E:\FFWS\ModelOutput\StructureFF\PIRDP.png','rb')
session.storbinary('STOR PIRDP.png', file)
file.close()
session.quit()
