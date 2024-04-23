<script lang="ts">
    import type { User } from "$lib/handlers/accountHandler";
    import type {Group} from "$lib/handlers/groupHandler";
    import type { FriendInfo } from "$lib/handlers/userHandler";

    export let groupInfo: Group;
    export let friendInfo: FriendInfo = {} as FriendInfo;
    export let isUsersSidebarOpen: boolean;
    export let isInfoSidebarOpen: boolean = false;
    export let userInfo: User;
    export let chatId: string;



</script>

<div class="chat-header d-flex justify-content-between align-items-center p-3 border-bottom">
    {#if groupInfo?.isGroup}
    <div class="header-content">
        <div>
            <h3>{groupInfo?.name}</h3>
            <i class="fa fa-users" aria-hidden="true" on:click={() => {isUsersSidebarOpen = !isUsersSidebarOpen; isInfoSidebarOpen = false;}}></i>
        </div>
        <div class="info-icon">
            <i class="fa fa-circle-info" aria-hidden="true" on:click={()=>{isInfoSidebarOpen = !isInfoSidebarOpen; isUsersSidebarOpen = false;}}></i>
        </div>
    </div>
    {:else}
        <div class="header-content">
            <h3>{friendInfo?.username}</h3>
            <div class="info-icon">
                <i class="fa fa-circle-info" aria-hidden="true" on:click={()=>isInfoSidebarOpen = !isInfoSidebarOpen}></i>
            </div>
         </div>
    {/if}
    <div>
    </div>
</div>

<style>
    .chat-header{
        position: relative;
        z-index: 15;
        width: 100%;
        min-height: 7rem;
        max-height: 7rem;
        border-bottom: 1px solid rgba(var(--center-channel-color-rgb), 0.12);
        background: var(--center-channel-bg);
        font-size: 14px;
        box-sizing: border-box;
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
        .header-content {
            display: flex;
            width:100%;
            justify-content: space-between;
        }
        .fa-circle-info{
            font-size: 1.5rem;
            
        }
        .info-icon {
            margin-left: auto;
            align-items: center;
            justify-content: center;
            display: flex;
        }
</style>