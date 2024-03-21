<script lang="ts">
    import { handleLeaveGroup, type Group, handleDeleteGroup } from "$lib/Handlers/groupHandler";

    export let groupInfo: Group;
    export let isUsersSidebarOpen: boolean;
    export let isCreator: boolean;
    export let chatId: string;

    function toggleSidebar() {
        isUsersSidebarOpen = !isUsersSidebarOpen;
    }

    async function leaveGroup(){
        await handleLeaveGroup(chatId);
        window.location.href = '/chat/home';

    }
    async function deleteGroup(){
        if (isCreator) {
            await handleDeleteGroup(chatId);
            window.location.href = '/chat/home';
        }
    }
</script>
<div class="chat-header d-flex justify-content-between align-items-center p-3 border-bottom">
    <div>
        <h3>{groupInfo?.name}</h3>
        <i class="fa fa-users" aria-hidden="true" on:click={toggleSidebar}></i>
    </div>
    <div>
        <button class="btn btn-danger" on:click={leaveGroup}>Leave</button>
        {#if isCreator}
        <button class="btn btn-danger" on:click={deleteGroup}>Delete</button>
        {/if}
    </div>
</div>

<style>
    .chat-header{
    position: relative;
    z-index: 15;
    width: 100%;
    height:100%;
    flex: 0 0 63px;
    border-bottom: 1px solid rgba(var(--center-channel-color-rgb), 0.12);
    background: var(--center-channel-bg);
    font-size: 14px;
}
    .chat-header h3{
        margin-bottom:2px;
    }
    .chat-header i{
        cursor:pointer;
        
        padding-top:4px;
        margin-bottom:8px;
        padding-bottom:4px;

        padding-left:2px;
        padding-right:2px;
        border-radius: 5px;
    }
        .chat-header i:hover{
            background-color: #b4b4b4;
        }
</style>