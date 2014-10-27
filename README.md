SyncIt
======

Synchronizes folders smartly

Run example:

D:\Sights\SyncIt>start SyncItAgent.exe C:\Users\ThisUser\Documents\SyncIt.xml

SyncIt.xml example:

<?xml version="1.0" encoding="utf-8" ?>
<SyncConfiguration>
	<Projects>
		<Project Name="TheProject" ListenChanges="true">
			<Folders>
				<Folder Source="D:\Projects\SourceProjectFolder" Destination="d:\InetPubSites\IISProjectFolder" Method="Hardlink">
					<SpecialCareItems>
						<Item Source="thisuser.web.config" Destination="Web.config" Method="Copy" />
					</SpecialCareItems>
				</Folder>
			</Folders>
		</Project>
	</Projects>
</SyncConfiguration>